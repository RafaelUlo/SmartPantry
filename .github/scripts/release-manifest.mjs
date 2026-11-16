import { writeFile } from "node:fs/promises";
import { join } from "node:path";

// Runs after semantic-release has published the GitHub release.
export async function success(_config, { commits, nextRelease, env }) {
  await writeFile(
    join(env.RUNNER_TEMP, "smartpantry-release.json"),
    JSON.stringify({
      version: nextRelease.version,
      tag: nextRelease.gitTag,
      commits: commits.map(({ hash }) => hash),
    }),
  );
}
