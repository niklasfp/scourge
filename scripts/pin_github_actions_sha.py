#!/usr/bin/env python3
from __future__ import annotations

import argparse
import base64
import datetime as dt
import json
import os
import random
import re
import time
import urllib.error
import urllib.parse
import urllib.request
from dataclasses import dataclass
from typing import Any


USES_LINE_RE = re.compile(r"^(?P<prefix>\s*uses:\s*)(?P<value>[^#\n]+?)(?P<suffix>\s*(?:#.*)?)$")
SHA_RE = re.compile(r"^[0-9a-fA-F]{40}$")
YAML_FILE_RE = re.compile(r"\.ya?ml$", re.IGNORECASE)


@dataclass
class PinChange:
    line_number: int
    before: str
    after: str
    action_reference: str
    pinned_sha: str


@dataclass
class WorkflowUpdate:
    path: str
    file_sha: str
    original_content: str
    updated_content: str
    changes: list[PinChange]


class GitHubApiError(RuntimeError):
    pass


class GitHubClient:
    def __init__(self, token: str, user_agent: str = "niklasfp-sha-pin-script") -> None:
        self._token = token
        self._user_agent = user_agent
        self._base_url = "https://api.github.com"

    def _request(self, method: str, path: str, body: dict[str, Any] | None = None) -> tuple[Any, dict[str, str]]:
        url = f"{self._base_url}{path}"
        data = None
        if body is not None:
            data = json.dumps(body).encode("utf-8")
        request = urllib.request.Request(url=url, method=method, data=data)
        request.add_header("Accept", "application/vnd.github+json")
        request.add_header("Authorization", f"Bearer {self._token}")
        request.add_header("User-Agent", self._user_agent)
        request.add_header("X-GitHub-Api-Version", "2022-11-28")
        if data is not None:
            request.add_header("Content-Type", "application/json")

        try:
            with urllib.request.urlopen(request) as response:
                payload = response.read().decode("utf-8")
                headers = dict(response.headers.items())
                if not payload:
                    return None, headers
                return json.loads(payload), headers
        except urllib.error.HTTPError as error:
            body_text = error.read().decode("utf-8", errors="replace")
            raise GitHubApiError(f"{method} {path} failed with {error.code}: {body_text}") from error

    @staticmethod
    def _next_link(headers: dict[str, str]) -> str | None:
        link_header = headers.get("Link")
        if not link_header:
            return None
        for part in link_header.split(","):
            sections = [section.strip() for section in part.split(";")]
            if len(sections) < 2:
                continue
            if sections[1] == 'rel="next"' and sections[0].startswith("<") and sections[0].endswith(">"):
                return sections[0][1:-1]
        return None

    def list_owned_repositories(self, owner: str) -> list[dict[str, Any]]:
        repositories: list[dict[str, Any]] = []
        next_url = f"{self._base_url}/users/{owner}/repos?type=owner&per_page=100"
        while next_url:
            path = next_url.replace(self._base_url, "")
            response, headers = self._request("GET", path)
            repositories.extend(response)
            next_url = self._next_link(headers)
        return repositories

    def get_repository(self, owner: str, repo: str) -> dict[str, Any]:
        response, _ = self._request("GET", f"/repos/{owner}/{repo}")
        return response

    def list_workflow_files(self, owner: str, repo: str, ref: str) -> list[dict[str, Any]]:
        try:
            response, _ = self._request("GET", f"/repos/{owner}/{repo}/contents/.github/workflows?ref={urllib.parse.quote(ref, safe='')}")
        except GitHubApiError as error:
            if " 404:" in str(error):
                return []
            raise

        if not isinstance(response, list):
            return []

        files: list[dict[str, Any]] = []
        for item in response:
            if item.get("type") != "file":
                continue
            path = item.get("path", "")
            if not YAML_FILE_RE.search(path):
                continue
            files.append(item)
        return files

    def get_file_content(self, owner: str, repo: str, path: str, ref: str) -> tuple[str, str]:
        encoded_path = urllib.parse.quote(path, safe="/")
        response, _ = self._request("GET", f"/repos/{owner}/{repo}/contents/{encoded_path}?ref={urllib.parse.quote(ref, safe='')}")
        content_raw = response.get("content", "")
        encoding = response.get("encoding")
        file_sha = response.get("sha")
        if encoding != "base64":
            raise GitHubApiError(f"Unsupported encoding for {owner}/{repo}/{path}: {encoding}")
        content = base64.b64decode(content_raw).decode("utf-8")
        return content, file_sha

    def resolve_commit_sha(self, owner: str, repo: str, ref: str) -> str:
        quoted_ref = urllib.parse.quote(ref, safe="")
        response, _ = self._request("GET", f"/repos/{owner}/{repo}/commits/{quoted_ref}")
        sha = response.get("sha", "")
        if not SHA_RE.match(sha):
            raise GitHubApiError(f"Could not resolve ref {owner}/{repo}@{ref} to a full SHA")
        return sha.lower()

    def get_branch_head_sha(self, owner: str, repo: str, branch: str) -> str:
        quoted_branch = urllib.parse.quote(branch, safe="")
        response, _ = self._request("GET", f"/repos/{owner}/{repo}/git/ref/heads/{quoted_branch}")
        return response["object"]["sha"]

    def create_branch(self, owner: str, repo: str, branch: str, base_sha: str) -> None:
        self._request(
            "POST",
            f"/repos/{owner}/{repo}/git/refs",
            {
                "ref": f"refs/heads/{branch}",
                "sha": base_sha,
            },
        )

    def update_file(self, owner: str, repo: str, path: str, branch: str, file_sha: str, content: str, message: str) -> None:
        encoded_path = urllib.parse.quote(path, safe="/")
        self._request(
            "PUT",
            f"/repos/{owner}/{repo}/contents/{encoded_path}",
            {
                "message": message,
                "content": base64.b64encode(content.encode("utf-8")).decode("ascii"),
                "sha": file_sha,
                "branch": branch,
            },
        )

    def create_pull_request(self, owner: str, repo: str, title: str, head: str, base: str, body: str) -> str:
        response, _ = self._request(
            "POST",
            f"/repos/{owner}/{repo}/pulls",
            {
                "title": title,
                "head": head,
                "base": base,
                "body": body,
            },
        )
        return response["html_url"]


def parse_action_reference(raw_value: str) -> tuple[str, str, str, str] | None:
    value = raw_value.strip()
    if not value:
        return None

    quote = ""
    if value[0] in {'"', "'"} and value[-1] == value[0]:
        quote = value[0]
        value = value[1:-1]

    if value.startswith("./") or value.startswith("../") or value.startswith("docker://") or value.startswith("${{"):
        return None

    if "@" not in value:
        return None

    target, ref = value.rsplit("@", 1)
    if not target or not ref:
        return None

    parts = target.split("/")
    if len(parts) < 2:
        return None

    action_owner = parts[0]
    action_repo = parts[1]

    return quote, target, ref, f"{action_owner}/{action_repo}"


def pin_workflow_content(content: str, resolve_sha: Any) -> tuple[str, list[PinChange]]:
    lines = content.splitlines(keepends=True)
    changes: list[PinChange] = []

    for index, line in enumerate(lines):
        match = USES_LINE_RE.match(line.rstrip("\n"))
        if not match:
            continue

        parsed = parse_action_reference(match.group("value"))
        if parsed is None:
            continue

        quote, target, ref, repo_identifier = parsed
        if SHA_RE.match(ref):
            continue

        sha = resolve_sha(repo_identifier, ref)
        new_value = f"{target}@{sha}"
        if quote:
            new_value = f"{quote}{new_value}{quote}"

        suffix = match.group("suffix")
        readable_suffix = suffix if suffix.strip() else f"  # {ref}"

        line_break = "\n" if line.endswith("\n") else ""
        new_line = f"{match.group('prefix')}{new_value}{readable_suffix}{line_break}"

        if new_line != line:
            lines[index] = new_line
            changes.append(
                PinChange(
                    line_number=index + 1,
                    before=line.rstrip("\n"),
                    after=new_line.rstrip("\n"),
                    action_reference=f"{target}@{ref}",
                    pinned_sha=sha,
                )
            )

    return "".join(lines), changes


def build_branch_name(prefix: str) -> str:
    timestamp = dt.datetime.utcnow().strftime("%Y%m%d%H%M%S")
    random_part = random.randint(1000, 9999)
    return f"{prefix}-{timestamp}-{random_part}"


def run(args: argparse.Namespace) -> int:
    token = args.token or os.getenv("GITHUB_TOKEN") or os.getenv("GH_TOKEN")
    if not token:
        print("ERROR: Missing GitHub token. Set --token or GITHUB_TOKEN/GH_TOKEN.")
        return 1

    client = GitHubClient(token=token)

    repositories: list[dict[str, Any]]
    if args.repositories:
        repositories = []
        for item in args.repositories:
            owner, repo = item.split("/", 1)
            repo_data = client.get_repository(owner, repo)
            repositories.append(repo_data)
    else:
        repositories = client.list_owned_repositories(args.owner)

    repositories = [repo for repo in repositories if repo.get("owner", {}).get("login", "").lower() == args.owner.lower()]
    repositories.sort(key=lambda item: item["full_name"].lower())

    sha_cache: dict[tuple[str, str], str] = {}

    def resolve_sha(repo_identifier: str, ref: str) -> str:
        key = (repo_identifier.lower(), ref)
        if key in sha_cache:
            return sha_cache[key]
        action_owner, action_repo = repo_identifier.split("/", 1)
        sha = client.resolve_commit_sha(action_owner, action_repo, ref)
        sha_cache[key] = sha
        time.sleep(args.request_delay_ms / 1000.0)
        return sha

    summary: dict[str, Any] = {"repositories": []}

    for repo in repositories:
        full_name = repo["full_name"]
        owner = repo["owner"]["login"]
        repo_name = repo["name"]
        default_branch = repo["default_branch"]

        repo_result: dict[str, Any] = {
            "repository": full_name,
            "default_branch": default_branch,
            "workflow_files": 0,
            "updated_files": 0,
            "changes": 0,
            "status": "no_changes",
            "pull_request": None,
            "details": [],
        }

        print(f"\n==> {full_name} (default: {default_branch})")

        workflows = client.list_workflow_files(owner, repo_name, default_branch)
        repo_result["workflow_files"] = len(workflows)

        if not workflows:
            repo_result["status"] = "no_workflows"
            print("  no workflow files found")
            summary["repositories"].append(repo_result)
            continue

        updates: list[WorkflowUpdate] = []
        for workflow in workflows:
            path = workflow["path"]
            content, file_sha = client.get_file_content(owner, repo_name, path, default_branch)
            updated_content, changes = pin_workflow_content(content, resolve_sha)
            if not changes:
                continue

            updates.append(
                WorkflowUpdate(
                    path=path,
                    file_sha=file_sha,
                    original_content=content,
                    updated_content=updated_content,
                    changes=changes,
                )
            )

            print(f"  {path}: {len(changes)} changes")
            for change in changes:
                print(f"    line {change.line_number}: {change.action_reference} -> {change.pinned_sha}")
                repo_result["details"].append(
                    {
                        "file": path,
                        "line": change.line_number,
                        "before": change.before,
                        "after": change.after,
                        "action": change.action_reference,
                        "sha": change.pinned_sha,
                    }
                )

        if not updates:
            print("  no unpinned action references found")
            summary["repositories"].append(repo_result)
            continue

        repo_result["updated_files"] = len(updates)
        repo_result["changes"] = sum(len(update.changes) for update in updates)

        if args.dry_run:
            repo_result["status"] = "dry_run_changes"
            print("  dry-run: no changes written")
            summary["repositories"].append(repo_result)
            continue

        target_branch = default_branch if args.direct_commit else build_branch_name(args.branch_prefix)
        if not args.direct_commit:
            head_sha = client.get_branch_head_sha(owner, repo_name, default_branch)
            client.create_branch(owner, repo_name, target_branch, head_sha)
            print(f"  created branch: {target_branch}")

        commit_message = args.commit_message.format(repository=full_name)
        for update in updates:
            client.update_file(owner, repo_name, update.path, target_branch, update.file_sha, update.updated_content, commit_message)
            print(f"  updated {update.path} on {target_branch}")

        if args.direct_commit:
            repo_result["status"] = "committed"
        else:
            pr_title = args.pr_title
            pr_body = args.pr_body_template.format(
                repository=full_name,
                default_branch=default_branch,
                changes=repo_result["changes"],
                files=repo_result["updated_files"],
            )
            pr_url = client.create_pull_request(owner, repo_name, pr_title, target_branch, default_branch, pr_body)
            repo_result["status"] = "pr_created"
            repo_result["pull_request"] = pr_url
            print(f"  pull request: {pr_url}")

        summary["repositories"].append(repo_result)

    print("\n=== Summary ===")
    for repo_result in summary["repositories"]:
        status = repo_result["status"]
        print(
            f"- {repo_result['repository']}: {status} "
            f"(workflows={repo_result['workflow_files']}, files={repo_result['updated_files']}, changes={repo_result['changes']})"
        )
        if repo_result.get("pull_request"):
            print(f"  PR: {repo_result['pull_request']}")

    if args.summary_file:
        with open(args.summary_file, "w", encoding="utf-8") as file:
            json.dump(summary, file, indent=2)
        print(f"\nWrote summary to {args.summary_file}")

    return 0


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        description="SHA-pin unpinned GitHub Action references in workflow files across repositories.",
    )
    parser.add_argument("--owner", default="niklasfp", help="GitHub owner to scan (default: niklasfp)")
    parser.add_argument(
        "--repositories",
        nargs="*",
        help="Optional explicit repositories to process, format: owner/repo",
    )
    parser.add_argument("--token", help="GitHub API token. Falls back to GITHUB_TOKEN or GH_TOKEN")
    parser.add_argument("--dry-run", action="store_true", help="Analyze and report changes without writing updates")
    parser.add_argument("--direct-commit", action="store_true", help="Commit directly to default branch instead of opening PRs")
    parser.add_argument("--branch-prefix", default="chore/pin-actions-sha", help="Prefix for generated branch names")
    parser.add_argument("--commit-message", default="chore: SHA-pin GitHub Actions refs in workflows ({repository})")
    parser.add_argument("--pr-title", default="chore: SHA-pin unpinned GitHub Actions refs")
    parser.add_argument(
        "--pr-body-template",
        default=(
            "This PR SHA-pins unpinned GitHub Actions references in workflow files.\n\n"
            "- Repository: `{repository}`\n"
            "- Base branch: `{default_branch}`\n"
            "- Updated workflow files: `{files}`\n"
            "- Updated action references: `{changes}`\n\n"
            "No action versions/tags were upgraded; each ref was pinned to the commit SHA for the existing ref."
        ),
    )
    parser.add_argument("--summary-file", help="Optional path to write JSON summary output")
    parser.add_argument("--request-delay-ms", type=int, default=100, help="Delay between resolve requests (default: 100ms)")
    return parser


if __name__ == "__main__":
    raise SystemExit(run(build_parser().parse_args()))
