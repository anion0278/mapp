Total of 5 new changes since v3.0.2.3

## 🚀 Features

- feature: WPF binding exceptions are now handled explicitly -> Author: anion0278
- feature: App Center integration -> Author: anion0278

## 🐛 Fixes

- fix: added no-CI tag for automated commits -> Author: anion0278
- fix: exceptions thrown when editing Pack Quantity, WarhouseProdCode or AmazonProdName in WPF for any entities except for Products -> Author: anion0278

## 🧰 Chores

- chore: Automated report -> Author: AutomatedRelease

## 📦 Uncategorized

- chores: updated readme [skip ci] -> Author: anion0278
- chores: improved ci/cd [skip ci] -> Author: anion0278
- Update ci_cd.yml -> Author: anion0278
- chroes: added changelog formatting settings -> Author: anion0278
- Update ci_cd.yml -> Author: anion0278
- Merge branch 'dev' -> Author: anion0278
- Merge branch 'master' of https://github.com/anion0278/mapp -> Author: anion0278

============================================================================================

Total of 15 new changes since v3.2.1.8:
## 🚀 Features

- feature: predefined shipping types for countries, disabled product name change in UI, fixed tests -> Author: anion0278

## 🛠️ Minor Changes

- change: customs declaration is remembered only if Invoice has a single item, added validation rule for its length -> Author: anion0278

## 🐛 Fixes

- fix: invalid amazon URL central due to case sensitivity -> Author: anion0278
- fix: always instantiating keyborad helper -> Author: anion0278
- fix: exception on empty Gift Wrap Tax value, changed UI -> Author: anion0278
- fix: item price calculation for EU countries -> Author: anion0278
- fix: missing class - naming -> Author: anion0278
- fix: changelog prepending [skip ci] -> Author: anion0278
- fix: max changelog length fix [skip ci] -> Author: anion0278
- fix: change log appending -> Author: anion0278

## 📄 Documentation

- doc: improved changelog formatting [skip ci] -> Author: anion0278
- doc: cleaned changelog [skip ci] -> Author: anion0278
- doc: improved changelog template [skip ci] -> Author: anion0278

## 🧰 Chores

- chore: Automated report [skip ci] -> Author: AutomatedRelease
- chore: Automated report [skip ci] -> Author: AutomatedRelease\nNew release on ${{MERGED_AT}}
Total of 19 new changes since v3.2.1.8:
## 🚀 Features

- feature: predefined shipping types for countries, disabled product name change in UI, fixed tests

## 🛠️ Minor Changes

- change: customs declaration is remembered only if Invoice has a single item, added validation rule for its length

## 🐛 Fixes

- fix: invalid amazon URL central due to case sensitivity
- fix: always instantiating keyborad helper
- fix: exception on empty Gift Wrap Tax value, changed UI
- fix: item price calculation for EU countries
- fix: missing class - naming
- fix: changelog prepending [skip ci]
- fix: max changelog length fix [skip ci]
- fix: change log appending
- fix: tax calculation
- fix: missing changelog step

[skip ci]

## 📄 Documentation

- doc: improved changelog formatting [skip ci]
- doc: cleaned changelog [skip ci]
- doc: improved changelog template [skip ci]
- doc: removed author name from commit description [skip ci]

## 🧰 Chores

- chore: Automated report [skip ci]
- chore: Automated report [skip ci]
- chore: Automated report [skip ci]

## 📦 Uncategorized

- chores: removed unused env var definition

[skip ci]
- Merge branch 'master' of https://github.com/anion0278/mapp
\nNew release on ${{MERGED_AT}}
Total of 1 new changes since v3.2.2.15:
## 🧰 Chores

- chore: Automated report [skip ci]

## 📦 Uncategorized

- Update CHANGELOG.md
- Update ci_cd.yml\nNew release on ${{MERGED_AT}}
Total of 3 new changes since v3.2.2.15:
## 🐛 Fixes

- fix: exception during csv currency parsing

## 🧰 Chores

- chore: Automated report [skip ci]
- chore: Automated report [skip ci]

## 📦 Uncategorized

- Update CHANGELOG.md
- Update ci_cd.yml
- Merge branch 'master' of https://github.com/anion0278/mapp\nNew release
Total of 1 new changes since v3.2.3.17:
## 🧰 Chores

- chore: Automated report [skip ci]

## 📦 Uncategorized

- Update changelog_settings.json

[skip ci]
- [tests] approval testing
Security - Replaced dangerous Path.Combine with Path.Join
- Merge branch 'master' into dev
- Updated projects to newer platform
- disabled UI test
- Stock quantity data conversion
Refactoring is needed!
- simple notificaiton about processing\nNew release
Total of 1 new changes since v3.3.1.19:
## 🧰 Chores

- chore: Automated report [skip ci]

## 📦 Uncategorized

- Fixed invalid paths
- Merge branch 'master' of https://github.com/anion0278/mapp\nNew release
Total of 1 new changes since v3.3.2.20:
## 🧰 Chores

- chore: Automated report [skip ci]

## 📦 Uncategorized

- Loading stock quantity updater config from json
- Merge branch 'master' of https://github.com/anion0278/mapp
- [feature] Added hyperlinks for marketplaces\nNew release
Total of 73 new changes since v3.0.1:
## 🚀 Features

- feature: added azure function for fetching github releases
- feature: set updates list to Azure function output
- feature: WPF binding exceptions are now handled explicitly
- feature: App Center integration
- feature: Invoice item groupping, better data visualization
- feature: post-export open folder setting attempts to reuse already opened folder if possible
- feature: invoice number column now works as link to seller central
- feature: predefined shipping types for countries, disabled product name change in UI, fixed tests
- feature: implement parsing from CZ paypal

Resolves #65, #64

## 🛠️ Minor Changes

- change: item price aggregation using operator
- change: tax price calculation invariant support
- change: better error notification for grids, disabled virtualization
- change: added validation rule for Invoice item, added country exception for moss codes
- change: added validation rule for Invoice VM
- change: Fody for VMs
- change: improved UI, unblocking datagrid allows to edit even if there are invalid cells
- change: improved project definitions
- change: all libs are included into single file app [skip ci]
- change: updated invoice and transactions configurations
- change: customs declaration is remembered only if Invoice has a single item, added validation rule for its length

## 🔎 Breaking Changes

- breaking: Fixed Paypal GPC transactions converter
- breaking: Add Shopify GPC config, changed Order ID parsing

Changed Order ID parsing to remove everything except for numbers and '-'

Resolves #75

## 🐛 Fixes

- fix: fixed azure funtion name
- fix: invalid project definition
- fix: duplicate assembly params
- fix: missing configs
- fix: added no-CI tag for automated commits
- fix: exceptions thrown when editing Pack Quantity, WarhouseProdCode or AmazonProdName in WPF for any entities except for Products
- fix: colon position [skip ci]
- fix: total invoice price calculation, invariants fullfilling, refactoring
- fix: invocie item validation rule
- fix: changelog formatting
- fix: hotfix - disabled App center
- fix: invalid amazon URL central due to case sensitivity
- fix: always instantiating keyborad helper
- fix: exception on empty Gift Wrap Tax value, changed UI
- fix: item price calculation for EU countries
- fix: missing class - naming
- fix: changelog prepending [skip ci]
- fix: max changelog length fix [skip ci]
- fix: change log appending
- fix: tax calculation
- fix: missing changelog step

[skip ci]
- fix: exception during csv currency parsing
- fix: fix window size and position settings when win is minimized

Resolves: #56

## 📄 Documentation

- doc: improved changelog formatting [skip ci]
- doc: cleaned changelog [skip ci]
- doc: improved changelog template [skip ci]
- doc: removed author name from commit description [skip ci]
- doc: Update README.md
- docs: Update README.md
- docs: Update README.md

[skip ci]
- docs: Update README.md

## 🧪 Tests

- test
- test: fixed Invoice converter tests
- test: fixed unvalid project reference in invoice tests
- test: fixed culture-dependent time settings
- test: disabled transactions tests
- test: removed problematic line

## 🧰 Chores

- chore: Automated report
- chore: Automated report [skip ci]
- chore: Automated report [skip ci]
- chore: Automated report [skip ci]
- chore: Automated report [skip ci]
- chore: Automated report [skip ci]
- chore: Automated report [skip ci]
- chore: Automated report [skip ci]
- chore: Automated report [skip ci]
- chore: Automated report [skip ci]
- chore: Automated report [skip ci]
- chore: Automated report [skip ci]
- chore: Automated report [skip ci]
- chore: Automated report [skip ci]

## 📦 Uncategorized

- First version of transactions converter
- Quantity of items is now remembered
- Customs declaration is now stored in mobilPhone
Edited declarations are stored in json and retrieved at invoice processing (only for UVzbozi)
Everything really needs refactoring
- Added transaction names for non-english amazons
Fixed discount adding up for multi-item invoices
Fixed Exception for custom shipping methods (such as Hermes)
Updated json configurations
- Added global keyboard hooks (F2+F4) and keyboard emulator for auto-filling the tracking number and current date
Tracking number is stored in settings
Tracking number can be changed in new Tab
- Added temp icon
Fixed Pack quantity calculations
Added calculations for discounts in transactions
Removing leading zeros from Var Sym
- Fixed exception during conversion of MX invoices
Fixed missing zeros at GPC reports when leading zeors were removed
Increased size of the buttons
- GPC - Refunds now have short ID from FIRST 10 sym of Order ID
Changed limit for city name length - 45 sym in Pohoda
- Added Nehterlands (NL) to GPC
- optional additional price parameters for any fees that should be included in the Total Invoice price
- Added conversion of Paypal summary to GPC
- Reading available transaction configs from JSON files
Requires separation to layers
Replaced Newton JSON nuget with Core nuget
- Added XML definition for future AutoUpdater functionality
- Cumulative updates definition test
- Deserializable update definition
- AutoUpdater for simple distribution of new release versions
- Updated version list
- Fixed AutoUpdater exception
- Final 2.0.0 changes
- Update test
- Update test
- Currency rates are no longer downloaded, instead they are read from csv file
Added information about v2.1.0
- LinesToSkipBeforeColumnNames is no longer needed - calculated automatically
- Added update definition for 2.2.0
autocomplete is now dependent on SKU code from Amazon
Saving Windows size/state is not supported yet
- fix for missing sku (Discount items dont have it)
- Comprehensive refactoring, however many changes still needed
Separated to assemblies
Added intergration tests
- Added MOSS
- Fixed price calculations
Updated test data
Added MOSS support
Added country-dependent VAT
- New update definitions
- Update definition v2.3.0a
- Update UpdatesDefinitions.json
- Update UpdatesDefinitions.json
- update defs fix
- Final update def fix
- Invoices - Fixed exception on File dialog cancellation
- Merge branch 'dev' of https://github.com/anion0278/mapp into dev
- Added user settings for main Window position, size and state
Fixed user settings loading
Changed unhandled exceptions handling
- Fixed integration tests
- Added user setting for opening the target folder after conversion
Fixed invoice conversion exceptions
Abstracted File operations
- Using Unity IoC Container for bootstrapping the project
Refactored ctors to accept config dir param from ConfigProvider
Fixed integration tests accordingly
- Extensive refactoring of invoice converter - separation of concerns
Separated dataAccess entities from business entities
Removed all code-behind from views
- Manual change window - refactoring - MVVM approach
View-first apporach with ViewModelLocator
Reworked the window itself
- Changed VS solution organization
Renamed project folders
Added WPF data validation
- Added definition for v3.0.0
- Hotfix - fixed payVat parameter for EU countries
- Merged master hotfix to dev
- Changed app title version formatting
- test change
- chores: created CI pipeline
- Rename dev.yml to ci.yml
- Create README.md
- Update ci.yml
- Update README.md
- Update ci.yml
- Update README.md
- chores: added CI to dev
- Merge branch 'dev' of https://github.com/anion0278/mapp into dev
- chores: added CI batch
- Update README.md
- chores: created CI/CD pipeline - manual trigger
- Create CHANGELOG.md
- Merge branch 'dev' of https://github.com/anion0278/mapp into dev
- Merge branch 'master' of https://github.com/anion0278/mapp
- chores: updated readme [skip ci]
- chores: improved ci/cd [skip ci]
- Update ci_cd.yml
- chroes: added changelog formatting settings
- Update ci_cd.yml
- Merge branch 'dev'
- Merge branch 'master' of https://github.com/anion0278/mapp
- chores: the last manual update of releases list
- tests: changed invoice converter tests
- tests: added design-time data for main VM; Fody prop change for manual change VM
- Merge branch 'master' into dev
- adapted CI CD for publishing [skip ci]
- Merge branch 'master' of https://github.com/anion0278/mapp
- chores: merge
- Merge branch 'master' of https://github.com/anion0278/mapp
- chores: removed unused env var definition

[skip ci]
- Merge branch 'master' of https://github.com/anion0278/mapp
- Update CHANGELOG.md
- Update ci_cd.yml
- Merge branch 'master' of https://github.com/anion0278/mapp
- Update changelog_settings.json

[skip ci]
- [tests] approval testing
Security - Replaced dangerous Path.Combine with Path.Join
- Merge branch 'master' into dev
- Updated projects to newer platform
- disabled UI test
- Stock quantity data conversion
Refactoring is needed!
- simple notificaiton about processing
- Fixed invalid paths
- Merge branch 'master' of https://github.com/anion0278/mapp
- Loading stock quantity updater config from json
- Merge branch 'master' of https://github.com/anion0278/mapp
- [feature] Added hyperlinks for marketplaces
- Migrate to MVVM Toolkit and Fody PropChanged

Close #1
- Separate viewmodels, migrate to VM-First

Related to: #4
- fix missing (ignored) files

related to #4
- Minor: migrated to autofac

Resolves: #52
- tests: updated tests

Related to: #52
- tests: implement pre-commit hook test, fix tests, fixed Azure function

Resolves: #45
- chores: Merged dev to master
- chores: Merged dev to master
- Merge branch 'dev' of https://github.com/anion0278/mapp into dev
- minor: window position, state, size are remembered

Resolves: #17
- minor: Added dynamic localization

Related to: #54
- Add app language selection

Related to #54
- refactor project structure
- Refactor architecture
- Merge pull request #58 from anion0278/dev

Iteration 2 - internalization and architecture changes
- fix main windows out of screen border

resolves #56
- Merge branch 'master' into dev
- Merge pull request #61 from anion0278/dev

Iteration 3
- Update README.md
- Merge pull request #62 from anion0278/anion0278-patch-1

Update README.md
- Add tab names dynamic localization using custom binding

Related to #54
- Merge branch 'dev' of https://github.com/anion0278/mapp into dev
- Merge pull request #63 from anion0278/dev

Iteration 4
- Delete Martin_app.sln.DotSettings
- Add integration tests for Transactions
- Merge pull request #76 from anion0278/dev

Dev
- Change number of parallel build threads