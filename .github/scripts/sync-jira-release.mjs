import { readFile } from "node:fs/promises";

export function issueFromBranch(branch) {
  return branch.match(/(?:^|[/_-])(SP-\d+)(?=$|[/_-])/i)?.[1].toUpperCase();
}

async function request(url, authorization, method = "GET", body) {
  const response = await fetch(url, {
    method,
    headers: {
      Authorization: authorization,
      Accept: "application/json",
      ...(body ? { "Content-Type": "application/json" } : {}),
    },
    body: body ? JSON.stringify(body) : undefined,
    signal: AbortSignal.timeout(30000),
  });
  if (!response.ok) {
    throw new Error(`${method} ${new URL(url).pathname}: HTTP ${response.status}`);
  }
  return response.status === 204 ? null : response.json();
}

export async function syncRelease(manifest, env, call = request) {
  const required = ["GITHUB_REPOSITORY", "GITHUB_TOKEN", "JIRA_BASE_URL", "JIRA_EMAIL", "JIRA_API_TOKEN"];
  for (const name of required) {
    if (!env[name]) throw new Error(`Missing ${name}`);
  }
  const github = `Bearer ${env.GITHUB_TOKEN}`;
  const jira = `Basic ${Buffer.from(`${env.JIRA_EMAIL}:${env.JIRA_API_TOKEN}`).toString("base64")}`;
  const api = env.GITHUB_API_URL || "https://api.github.com";
  const tickets = new Set();
  for (const hash of manifest.commits) {
    for (let page = 1; ; page++) {
      const pulls = await call(`${api}/repos/${env.GITHUB_REPOSITORY}/commits/${hash}/pulls?per_page=100&page=${page}`, github);
      for (const pull of pulls) {
        if (!pull.merged_at || pull.base.ref !== "main") continue;
        const issue = issueFromBranch(pull.head.ref);
        if (issue) tickets.add(issue);
      }
      if (pulls.length < 100) break;
    }
  }
  const base = env.JIRA_BASE_URL.replace(/\/+$/, "");
  if (new URL(base).protocol !== "https:") throw new Error("Jira URL must use HTTPS");
  const buildField = env.JIRA_BUILD_FIELD || "customfield_10048";
  const resultField = env.JIRA_RESULT_FIELD || "customfield_10047";
  const reportField = env.JIRA_REPORT_FIELD || "customfield_10049";
  for (const issue of [...tickets].sort()) {
    // A published version requires a new test execution.
    await call(`${base}/rest/api/3/issue/${issue}`, jira, "PUT", {
      fields: {
        [buildField]: manifest.version,
        [resultField]: { value: "Not Run" },
        [reportField]: null,
      },
    });
    console.log(`${issue}: release ${manifest.version}, tests Not Run`);
  }
  return [...tickets].sort();
}

if (process.argv[1]?.endsWith("sync-jira-release.mjs")) {
  const manifest = JSON.parse(await readFile(process.argv[2], "utf8"));
  await syncRelease(manifest, process.env);
}
