# Corporate Chaos

A turn-based business management simulation game built with WPF and .NET 8.0.

Take the helm of a company and steer it through 30 years of corporate life — one quarter at a time. Hire employees, manage departments, make executive decisions, survive random crises, and try to build a billion-dollar empire.

## Features

- **Story Mode** — Learn the ropes with Secretary Joan guiding you through a narrative-driven tutorial with branching dialogue and character relationships
- **Sandbox Mode** — Jump straight in with all features unlocked, in a 120-quarter challenge or endless free play
- **Dynamic Chaos Engine** — Random events driven by your company stats keep every playthrough unpredictable
- **Employee Management** — Hire, assign, transfer, and fire employees across Marketing, Operations, Finance, HR, IT, and Research
- **Executive Decisions** — Marketing campaigns, R&D investments, company retreats, crisis consultants, business loans, and cost cutting
- **Detailed Financials** — Quarterly revenue/expense breakdowns, financial reports, and performance tracking
- **Fully Customizable** — Edit JSON data files to change employee names, events, game balance, and job descriptions without any coding
- **Completely Offline** — No internet, no accounts, no telemetry. A single desktop application that reads and writes local files

## Download

Check the [Releases](https://github.com/Theo-retical808/corpo_chaos/releases) page for the latest installer.

## Build from Source

**Prerequisites:** [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

```bash
cd corporate_chaos
dotnet build        # Build
dotnet run          # Run
```

To create a self-contained release (no .NET required on target machine):

```bash
dotnet publish -c Release -r win-x64 --self-contained -o publish
```

See [installer/README.md](installer/README.md) for creating the installer with Inno Setup.

## Customization

Corporate Chaos is designed to be modifiable without writing code. Edit the JSON files in the `data/` folder:

| File | What It Controls |
|------|-----------------|
| `names.json` | Employee first names (male/female) and last names |
| `events.json` | All random event descriptions, crises, scandals, positive PR |
| `gamebalance.json` | Starting values, costs, revenue formulas, event probabilities |
| `positions.json` | Department job descriptions and skill keywords |

See [docs/CUSTOMIZATION_GUIDE.md](docs/CUSTOMIZATION_GUIDE.md) for the full guide.

## Documentation

- [Technical Decisions & Architecture](docs/TECHNICAL_DECISIONS.md) — Why C#, why WPF, why this architecture, pros and cons
- [Customization Guide](docs/CUSTOMIZATION_GUIDE.md) — How to modify the game without code

## AI-Generated Content Disclosure

Portions of this software were developed with the assistance of generative AI tools. This includes source code, game logic, UI layouts, story dialogue, event descriptions, employee names, character portrait images, and documentation. All AI-generated content has been reviewed and curated by the developer.

## License

This project is provided as-is for entertainment and educational purposes.

## Status

**v0.1.2 Beta** — Playable but under active development. Story mode is being expanded. Bug reports welcome at [Issues](https://github.com/Theo-retical808/corpo_chaos/issues).
