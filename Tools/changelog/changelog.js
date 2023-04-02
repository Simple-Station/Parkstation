const fs = require("fs");
const yaml = require("js-yaml");
const axios = require("axios");

if (process.env.GITHUB_TOKEN) axios.defaults.headers.common["Authorization"] = `Bearer ${process.env.GITHUB_TOKEN}`;

async function main() {
    // Get PR details
    const pr = await axios.get(`https://api.github.com/repos/${process.env.GITHUB_REPOSITORY}/pulls/${process.env.PR_NUMBER}`);
    const { merged_at, body, user } = pr.data;

    // Get author
    const HeaderRegex = /^\s*(?::cl:|🆑) *([a-z0-9_\- ]+)?\s+/im;
    const headerMatch = HeaderRegex.exec(body);
    if (!headerMatch) {
        console.log("No changelog entry found, skipping");
        return;
    }

    let author = headerMatch[1];
    if (!author) {
        console.log("No author found, setting it to author of the PR\n");
        author = user.login;
    }

    // Get all changes
    const EntryRegex = /^ *[*-]? *(add|remove|tweak|fix): *([^\n\r]+)\r?$/img;
    const matches = [];
    entries = [];

    for (const match of body.matchAll(EntryRegex)) {
        matches.push([match[1], match[2]]);
    }

    if (!changes)
    {
        console.log("No changes found, skipping");
        return;
    }

    // Check change types and construct changelog entry
    changes.forEach((entry) => {
        let type;

        switch (entry[0].toLowerCase()) {
            case "add":
                type = "add";
                break;
            case "remove":
                type = "remove";
                break;
            case "tweak":
                type = "tweak";
                break;
            case "fix":
                type = "fix";
                break;
            default:
                break;
        }

        if (type) {
            entries.push({
                type: type,
                message: entry[1],
            });
        }
    });

    // Time is something like 2021-08-29T20:00:00Z
    // Time should be something like 2023-02-18T00:00:00.0000000+00:00
    let time = merged_at;
    if (time)
    {
        time = time.replace("z", ".0000000+00:00");
    }
    else
    {
        console.log("Pull request was not merged, skipping");
        return;
    }

    // Construct changelog entry
    const entry = {
        author: author,
        changes: entries,
        id: parseInt(process.env.PR_NUMBER),
        time: time,
    };

    // Read changelogs.yml file
    const file = fs.readFileSync(
        `../../${process.env.CHANGELOG_DIR}`,
        "utf8"
    );
    const data = yaml.load(file);

    const changelogs = data && data.Entries ? Array.from(data.Entries) : [];

    // Check if 'Entries:' already exists and remove it
    const index = Object.entries(changelogs).findIndex(([key, value]) => key === "Entries");
    if (index !== -1) {
        changelogs.splice(index, 1);
    }

    // Add the new entry to the end of the array
    changelogs.push(entry);
    const updatedChangelogs = changelogs;

    // Write updated changelogs.yml file
    fs.writeFileSync(
        `../../${process.env.CHANGELOG_DIR}`,
        "Entries:\n" +
            yaml.dump(updatedChangelogs, { indent: 2 }).replace(/^---/, "")
    );

    console.log(`Changelog updated with changes from PR #${process.env.PR_NUMBER}`);
}

main();
