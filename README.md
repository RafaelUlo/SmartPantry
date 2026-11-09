# SmartPantry 🥬

**SmartPantry** is a university software engineering project focused on helping households manage their food inventory, discover recipes using available ingredients, plan grocery shopping, and reduce food waste.

> **Project status:** In development · **Current planning milestone:** Sprint 01 (9–20 November 2026)

## Overview

SmartPantry brings household food management into a single mobile experience. Users can create a household profile, record products in their pantry, find meals based on available ingredients, and track what needs to be purchased or consumed.

### Planned capabilities

- **Household onboarding:** Set up household name, serving size, dietary preferences, allergens, and excluded ingredients.
- **Profile management:** Edit preferences and manage household profiles.
- **Pantry dashboard:** View and manage available food products.
- **Barcode scanning and product lookup:** Scan packaged goods and retrieve product information where supported by external APIs.
- **Recipe discovery:** Find recipes based on ingredients, dietary preferences, and restrictions.
- **Recipe book and collections:** Save and organize recipes.
- **Cook Mode and timers:** Follow cooking instructions with helpful timers.
- **Meal completion:** Update pantry quantities after preparing meals.
- **Grocery shopping list:** Keep track of products to buy.
- **Settings and adjustments:** Manage application preferences and inventory corrections.

> These features describe the planned product scope; they are not all implemented yet.

## Technology stack

| Area | Technology | Status |
| --- | --- | --- |
| Mobile application | **.NET MAUI**, C# and XAML | **Selected** |
| Mobile platforms | Android and iOS | Target platforms |
| Source control | Git and GitHub | Planned setup |
| Project management | Jira (Scrum) | In use |
| Requirements and acceptance criteria | BDD / Gherkin (`Given / When / Then`) | In use |
| Automated testing | Testing integration with Jira and GitHub | Planned / to be validated |
| Backend API, database, hosting | To be documented after technical selection | Not finalized in this README |
| Product and recipe data | External APIs to be evaluated | Not finalized |

### Why .NET MAUI?

.NET MAUI was chosen after considering Flutter, React Native, and separate native Android/iOS implementations. All are viable, but **C# and .NET are the project's primary developer's area of professional specialization**. Using .NET MAUI reduces the learning curve, supports a shared Android/iOS codebase, and leaves more time to deliver the product's core features. Platform-specific requirements, especially barcode scanning and iOS builds, will be validated during development.

## Getting started

The repository is being initialized. The exact solution paths and commands will be updated when the .NET MAUI project is committed.

### Prerequisites

- A supported .NET SDK and .NET MAUI workload, matching the version selected in the repository.
- Visual Studio or another supported .NET MAUI development environment.
- Android SDK/emulator or a physical Android device.
- For iOS builds and signing, a compatible macOS/Xcode environment.

Once the solution is available, the typical workflow will be:

```bash
# Clone the repository (replace with the real GitHub URL)
git clone <repository-url>
cd SmartPantry

# Inspect the repository for the solution and target framework
# Follow the platform-specific run instructions added with the app source.
```

No build command is documented as verified until the application solution is committed and tested.

## Architecture principles

- Separate presentation, application logic, domain concepts, and infrastructure concerns where beneficial.
- Keep mobile UI components focused on presentation and user interaction.
- Use dependency injection and testable abstractions for external services.
- Treat third-party product and recipe data as untrusted input; validate and handle missing results.
- Store and transmit household dietary/allergen preferences carefully, using least-privilege access and appropriate data protection.
- Avoid coupling core functionality to a single external product or recipe provider.
- Design for accessible screens, clear validation, and resilient network behavior.

The final solution structure will be documented after initial implementation; the principles above are guidance, not a claim about existing code.

## Git branching strategy — Trunk-Based Development

SmartPantry uses **trunk-based development**, with `main` as the single long-lived integration branch.

### Branch rules

1. **`main` is always the integration trunk.** Keep it buildable and preferably releasable.
2. Create **short-lived branches** from the latest `main` for each Jira ticket.
3. Open a **small pull request (PR)** early; aim to merge frequently rather than keeping branches open for long periods.
4. Require review and passing automated checks before merging to `main` (once checks are configured).
5. Prefer **squash merging** to keep history concise and traceable.
6. Delete merged branches. Avoid long-lived `develop`, `release/*`, or team-specific integration branches.
7. Use **feature flags** or small vertical slices for unfinished work; do not merge broken or exposed features.
8. Never commit secrets, signing keys, local credentials, or environment-specific configuration containing sensitive data.

### Naming convention

```text
feature/SP-123-initial-onboarding
fix/SP-124-household-name-validation
chore/SP-125-configure-ci
docs/SP-126-architecture-decision
```

### Typical contribution flow

```bash
git checkout main
git pull --ff-only origin main
git switch -c feature/SP-123-initial-onboarding

# Make a focused change, add tests, and commit
git add .
git commit -m "feat(onboarding): add initial household setup"
git push -u origin feature/SP-123-initial-onboarding
```

Then open a PR targeting `main`, link the Jira ticket, complete the review/checks, and squash merge.

### Branch protection (to configure)

- Disallow direct pushes to `main` for ordinary contributors.
- Require pull requests and at least one approval where available.
- Require successful build and test checks before merge.
- Resolve review conversations before merging.
- Prevent force pushes and deletion of `main`.

**Important:** These are the intended repository policies. Their activation must be verified in GitHub; free-plan and repository-visibility restrictions may affect available enforcement options.

## Commit and pull request conventions

Use clear, scoped commit messages, preferably following **Conventional Commits**:

```text
feat(pantry): add inventory list
fix(onboarding): reject blank household names
test(onboarding): cover household validation
chore(ci): configure Android build
docs(readme): update development instructions
```

PRs should include:

- A concise description and linked Jira issue.
- Screenshots or a short recording for UI changes, where useful.
- Tests added/updated and how they were run.
- Any dependencies, breaking changes, or remaining limitations.

## Project management and Scrum

Work is tracked in **Jira** using a two-week Scrum cadence. The planned ceremonies are:

| Ceremony | Cadence |
| --- | --- |
| Sprint Planning | Every two weeks, Monday |
| Daily Stand-up | Daily, after 18:00 |
| Backlog Refinement | Wednesday |
| Technical Refinement | Wednesday |
| Sprint Review | Every two weeks, Friday |
| Sprint Retrospective | Every two weeks, Friday |

### Backlog refinement

**Functional Backlog Refinement → Technical Refinement → Ready**

A story is considered ready when its scope, acceptance criteria, dependencies, and implementation approach are sufficiently understood. Technical subtasks may be created during refinement.

### Ticket workflow

**Planning → In Progress → Review → Testing → Done**

The intended quality gate requires relevant automated tests to pass before a ticket reaches `Done`. Jira/GitHub/test-tool enforcement is a planned integration and must be verified before it can be relied upon.

### Estimation

Both metrics use the Fibonacci scale **1, 2, 3, 5, 8, 13, 21**, but measure **different dimensions**:

- **Business Value (BV):** Relative value delivered to the user/product; 1 = residual, 21 = critical.
- **Story Points (SP):** Relative effort, complexity, and uncertainty; 1 = very simple, 21 = too large and normally requiring decomposition.

Stories at **13 SP** should be reviewed for splitting; **21 SP** stories should generally be broken down before implementation. Priority considers value, effort, risk, and dependencies rather than relying on a formula alone.

## Documentation

Recommended project documentation locations:

- **Jira:** Epics, stories, tasks, estimates, sprint status, and dependencies.
- **Confluence (or another team wiki):** Meeting minutes, architectural decisions, and sprint summaries.
- **Restricted OneDrive/SharePoint storage:** Authorized meeting recordings and original transcripts.
- **Repository `/docs`:** Technical decisions, architecture diagrams, API contracts, and contributor instructions.

Do not publish private meeting recordings, personal data, API keys, or restricted university material in a public repository.

## Roadmap

1. **Foundation:** Repository, mobile stack, CI/testing, and initial onboarding.
2. **Household and pantry:** Profile management, dashboard, inventory, and barcode lookup.
3. **Recipe experience:** Discovery, recipe collections, cooking flow, and timers.
4. **Inventory lifecycle:** Meal completion, quantity adjustments, and shopping list.
5. **Hardening:** Cross-platform QA, accessibility, security, performance, and release preparation.

Roadmap sequencing is indicative and may change after backlog refinement.

## Contributing

Contributions should be linked to a Jira ticket, follow the trunk-based branching strategy, include relevant tests, and use a pull request. Discuss architectural changes in a short decision record under `/docs/decisions` before implementing large cross-cutting changes.

## License

**To be determined.** Until a license is added, do not assume that the repository grants open-source reuse rights.

---

*SmartPantry — Software Engineering / TAES university project.*
