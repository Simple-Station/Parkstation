const fs = require("fs");
const yaml = require("js-yaml");
const axios = require("axios");

// Using a token is optional, but will greatly reduce the chance of hitting the rate limit
if (process.env.GITHUB_TOKEN) axios.defaults.headers.common["Authorization"] = `Bearer ${process.env.GITHUB_TOKEN}`;

(async () => {
    // Debug
    console.log(`PR Number: ${process.env.PR_NUMBER}`);
    console.log(`Using Token: ${process.env.GITHUB_TOKEN != undefined && process.env.GITHUB_TOKEN != ""}`);
    console.log(`Repository: ${process.env.GITHUB_REPOSITORY}`);
    console.log(`Changelog Directory: ${process.env.CHANGELOG_DIR}`);
    console.log("\n");

    // Get PR details
    const pr = await axios.get(`https://api.github.com/repos/${process.env.GITHUB_REPOSITORY}/pulls/${process.env.PR_NUMBER}`);
    const { merged_at, body } = pr.data;

    // Get author
    const HeaderRegex = /^\s*(?::cl:|🆑) *([a-z0-9_\- ]+)?\s+/im;
    const headerMatch = HeaderRegex.exec(body);
    if (!headerMatch) {
        console.log("No changelog entry found, skipping");
        return;
    }

    let author = headerMatch[1];
    if (!author) {
        console.log("No author found, setting it to 'Untitled'");
        author = "Untitled";
    }
    else console.log(`Author: ${author}`);
    console.log("\n");

    // Get all changes
    entries = [];
    getAllChanges(body).forEach((entry) => {
        let type;

        switch (entry[1].toLowerCase()) {
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
                message: entry[2],
            });
        }
    });

    console.log(`Found ${entries.length} changes`);
    console.log("\n");

    // time is something like 2021-08-29T20:00:00Z
    // time should be something like 2023-02-18T00:00:00.0000000+00:00
    let time = merged_at;
    time = time.split("T")[0];
    time = time + "T00:00:00.0000000+00:00";
    console.log(`Time: ${time}`);
    console.log("\n");

    // Construct changelog entry
    const entry = {
        author: author,
        changes: entries,
        id: parseInt(process.env.PR_NUMBER),
        time: time,
    };

    console.log("Changelog entry:");
    console.log(entry);
    console.log("\n");

    // // Read changelogs.yml file
    // console.log("Reading changelogs file");
    // const file = fs.readFileSync(
    //     process.env.CHANGELOG_DIR,
    //     "utf8"
    // );
    // const data = yaml.load(file);

    // const changelogs = data && data.Entries ? Array.from(data.Entries) : [];

    // // Check if 'Entries:' already exists and remove it
    // const index = Object.entries(changelogs).findIndex(([key, value]) => key === "Entries");
    // if (index !== -1) {
    //     changelogs.splice(index, 1);
    // }

    // // Add the new entry to the end of the array
    // changelogs.push(entry);
    // const updatedChangelogs = changelogs;

    // // Write updated changelogs.yml file
    // console.log("Writing changelogs file");
    // fs.writeFileSync(
    //     process.env.CHANGELOG_DIR,
    //     "Entries:\n" +
    //         yaml.dump(updatedChangelogs, { indent: 2 }).replace(/^---/, "")
    // );

    // Instead of reading the changelogs file, just append the new changelogs entry to the end of the file
    console.log("Writing changelogs file");
    await fs.appendFile(
        process.env.CHANGELOG_DIR,
        yaml.dump(entry, { indent: 2 }).replace(/^---/, "")
    );

    console.log(`Changelog updated with changes from PR #${process.env.PR_NUMBER}`);
})();

function getAllChanges(description) {
    const EntryRegex = /^ *[*-]? *(add|remove|tweak|fix): *([^\n\r]+)\r?$/im;

    let changes = [];
    let match;

    while ((match = EntryRegex.exec(description))) {
        changes.push(match);
    }

    return changes;
}
