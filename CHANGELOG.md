# Auto-Reference Changelog

## 2.3.1

- Fixed documentation to include the correct current git tag.
- Add the license to the site and a link to it in `package.json`

## 2.3.0

### New

- Saving a prefab now also performs syncing if syncing on scene save is enabled.
- Added new `SyncMode` actions:
    - `SyncMode.Expanded` always retrieves and validates references, but allows extra values that satisfy the filters.
    - `SyncMode.ExpandedPermissive` always retrieves and validates references, but allows extra values without
      validation.

### Changes

- `SyncMode` has new naming conventions. Values have been renamed to be shorter, more consistent,
  and more unambigious as to their functionality. Prior values are deprecated but still allowed for now.
    - `GetIfEmpty` was renamed to `IfEmptyPermissive`
    - `ValidateOrGetIfEmpty` was renamed to `IfEmpty`
    - `AlwaysGetAndValidate` was renamed to `Always`

The reasoning is simple: All modes except for `ValidateOnly` do retrieval. All *retrieved* values are always validated.
So, including "Get" was redundant, and including "Validate" in some names but not all was confusing. Moreover the
suffix `Permissive` was added to imply any *additional* values (i.e. those that were not retrieved) are not validated.

### Fixes

- Fix very silly bug with `AutoReferenceEditor` always showing an error when inherited.
- Fix compatibility for Unity version 2021.1.

The fix for Unity 2020.3 in Auto-Reference 2.2.0 should have also been applied for Unity version 2021.1.
This is now fixed. However, 2021.1 required additional fixes that 2020.3 did not require as it contained some
back-ported features we relied on, due to it being an LTS release.

### Documentation updates

On top of new information that was added for the new features, the documentation went through a major overhaul:
- New style for inline code elements
- Blockquotes with a custom style
- Copy buttons for git links
- Fixed several typos, broken sentences, and misplaced punctuation marks.
- Removed some outdated information and fixed broken links that referred to moved/changed sections
- Updated the wording of several sentences for clarity
- Improved the appearance of tables to prevent them from overflowing the page

## 2.2.1

### Changes

This version does not add any new features, but it improves the Auto-Reference Window experience:

- Use alternating background for rows.
- Display vertical dividers between columns (can be turned off)
- New resizing strategy for columns:
    - Columns now resize to fit the window by default. Old behavior (overflow) is still available.
    - Columns can be proportionally resized when resizing the window (off by default)
- Display cell contents on hover. This is useful when the column is too narrow to display all the contents.
- Other visual refinements.

Access column settings by right-clicking on the column headers.

## 2.2.0

### New

- Fix support for Unity version 2020.3. There are currently no plans to support any versions earlier
  than 2020.3.

### Changes

- Package supported Unity version was bumped to 2020.3 from 2018.3. This was incorrect because only versions
  2021.1 and above were working prior to this update.
- Minor changes to the Auto-Reference Window header appearance.

## 2.1.0

### New

- Added `ISyncObserver` interface to allow sync callbacks on non-Unity types, as long as they exist as direct fields in
  `MonoBehaviour` scripts.
- Added `AutoReference.SyncData` family of methods to control the syncing of `ISyncObserver` objects that
  aren't direct fields on scripts, or they exist as elements within `List<T>` or `T[]` fields.

### Changes

- Update the Auto-Reference Window to remove deprecated warnings for Unity 6.2+
- Removed tracking of currently syncing scripts. This was a remnant of long-obsolete code and had stopped working
  properly anyway.
- Slight improvements in usage warnings and errors.
- Slight visual improvements in the Auto-Reference Window.
- Some internal changes.

## 2.0.3

- Change assembly references to reflect namespaces. This is possibly a breaking but minor change.
- Small internal changes

## 2.0.2

- Fix issues with some exception formatting combinations in older Unity versions.

## 2.0.1

This is a small update that improves how exceptions are logged. This only affects exceptions thrown while syncing a
field or during any `OnAfterSync` callbacks.

### Fixed

- Error hyperlinks now work properly for Unity versions 6+

_Unity changed how console hyperlinks are handled in version 6, which prevented the links from taking the user to the
appropriate line._

### New

- Options to enable and control formatting for exceptions thrown during syncing or `OnAfterSync` callbacks.

## 2.0.0

This update introduces several breaking changes, resolves minor issues, and adds new features. It represents a
culmination of key improvements identified during use of the toolkit.

### Breaking Changes

- All previous preference attributes have been removed and replaced by a Project Settings page and a Preferences page.
- Support for keeping empty values in the inspector has been dropped and replaced by a new feature (see Sync Toggle
  in New features below). This feature was a workaround to inconvenient behavior sometimes observed when syncing
  while editing would block the user from certain actions. However:
    - This solution was hacky and did not fix all problems associated with automatic syncing while inspecting.
    - It made the code a harder to understand particularly when extending the `AutoReferenceEditor` class.
    - `AutoReference.Cleanup` was removed as a result.
- Due to the above changes, some options were removed from SyncPreferences while it also contains some new ones.
- `SyncMode` and `ContextMode` can no longer be provided as constructor parameters to classes deriving from
  `AutoReferenceAttribute`. The user can now provide a `[SyncOptions(...)]` attribute to pass these options instead.
- `IOnAfterSync` was removed to simplify the handling of after sync callbacks.
- `ApplyFiltersAttribute` is now internal and no longer visible to the user. It will still be added implicitly like
  before. There is no need to add it manually anymore after moving `ContextMode` out of the attribute constructors.
  Keeping it internal allows the simplification of some error checking logic.
- `bool ValidateExisting(Object obj)` is removed from `AutoReferenceAttribute`. The user should now override
  `IEnumerable<Object> ValidateObjects(IEnumerable<Objects> objects)` which allows for more control.

### Behavior changes (possibly breaking):

- Scripts now only sync themselves in the inspector instead of the whole game object.
- AutoReferenceEditor was revamped.
    - Now using AutoReferenceDefaultEditor as a fallback.
    - AutoReferenceEditor always supports syncing even with integration disabled.
- Some undocumented classes may have moved or become internal.

### New

- Added `FindInParent` Attribute, which is a new core attribute to search for components under a parent of a specific
  type. It also allows filtering the parent via a callback method.
- Added a sync toggle button in the inspector which is displayed for scripts with auto-reference functionality and
  allows the user to temporarily disable syncing during editing. This is also supported in all 3rd party tool
  integrations. New preferences:
    - Modify the look and position of the sync toggle button.
    - Individually enable/disable 3rd party tool integration. The preferencesse will only be visible
      if any of the corresponding tools (Odin, Editor Toolbox, or TriInspector) are detected.
    - New logging options to fine-tune logging behavior.
- New buttons in the AutoReferenceWindow to quickly open settings and documentation.
- Add an option to ignore only the current component when using `[IgnoreSelf]` instead of the whole transform.

### Fixes

- After-sync callbacks no longer show a spurious error log telling the user to check the Auto-Reference Window.
- A fix to a problem with Component **↔** GameObject adaptations that would throw null reference exceptions (e.g.
  using the `[IgnoreSelf]` filter on a `GameObject` field).

## 1.2.8

### Fixes

- Fixed an issue where private callbacks weren't used by child classes.
- Fixed an issue that caused an exception sometimes when ApplyFiltersAttribute was used implicitly.

## 1.2.7

### Fixes

- Fixed Unity Editor locking when opening the Auto-Reference Window without any MonoBehaviour scripts in the project.

## 1.2.6

### Fixes

- Fixed regression with base fields not always getting synced.

## 1.2.5

### New

- `SkipSyncOnFilterError` option to skip syncing a field if any of its filters have errors.

### Changes

- Usage errors in `TagAttribute` and `LayerAttribute` were changed to warnings. Intuitively the user should expect that
  no objects should pass through if they're filtered through an invalid tag or layer. This behaviour is now identical
  to the old behaviour before validation was introduced.

### Improvements

- Minor visual improvements to the Auto-Reference Window.
- Auto-Reference Window now shows number of scripts with auto-reference info isntead of just the total number
  of all scripts

### Fixes

- Fixed Auto-Reference glitch that showed the wrong text color for "No usage errors or warnings detected" when the
  project was loaded.

## 1.2.4

### New

- Added validation for `TagAttribute` and `LayerAttribute` to show a warning or error for invalid tags/layers.

### Changes

- Clear Cache log message was simplified.

## 1.2.3

### Fixes

- Change detection has been rewritten from scratch with a new method using Unity's built-in serialized property data
  comparison. Hopefully this will fix all change detection issues once and for all. This is an issue that affected
  types that contain after-sync callbacks.

## 1.2.2

### Improvements

`TypeConstraint` now shows an error or warning for invalid usage, depending on the issue.

### Fixes

`TypeConstraint` now works properly.

## 1.2.1

### Fixes

Fix some serializable enum values throwing exceptions during `OnAfterSync` callbacks.

## 1.2.0

### Summary

This version focuses heavily on improving the overall experience of dealing with invalid usages. Errors and warnings
have become more concise and streamlined, and they no longer pollute the log console with repeated
messages while syncing. Instead, the user will see a short error or warning on the console and check out the brand new
Auto-Reference Window for more information (which also allows quickly jumping to the problematic files from there).
It's possible to turn the old behaviour back on, but it's now disabled by default.

### New

- Auto-Reference Report Window which displays useful information:
    - All warnings and errors in auto-reference usages
    - Statistics about auto-reference fields
- More logging of invalid usages, such as warnings when attempting to sync invalid fields.
- Add LogToConsole setting which can control Auto-Reference console logging.
- Add option to interrupt build if on-build syncing produced any usage warnings.

### Changes

- Primary attributes are now initialized during retrieval of auto-reference information instead of when syncing for the
  first time.
- Major changes to the logging system.

### Fixes

- Fixed derived types syncing inherited fields multiple times.
- Fixed errors with `OnAfterSync` callbacks throwing in certain situations.
- Sync callbacks are now more accurately retrieved and duplicate callbacks (due to user error) are automatically
  removed.
- Properly handle exceptions that can happen in after sync callbacks.

### Deprecations

- Dropped support for `Sync` and `SyncAllFields` attributes. Now it's always enabled when `OnAfterSync` exists.

## 1.1.1

### New

- Added option to interrupt build if on-build syncing produced any errors.
- Added menu option to clear cache.

## 1.1.0

This update focuses on adding new attributes and features but also fixes some breaking bugs.

### Fixes

- Fixed implicit Transform ↔ GameObject conversions which erroneously discarded objects in some cases.
- Fixed Scene syncing that skipped some objects.

### New

- `GetInSiblings` primary attribute.
- `GetInChildren` and `GetInParent` now have a `MaxDepth` property.
- `ContainsInChildren` and `ContainsInParent` filters which also have a `MaxDepth` property.
- `IgnoreNested` filter which discards components that are children of any other component in the input.

### Improvements

- Improved exception logging during syncing to include hyperlinks to files.
- Other minor internal improvements.

## 1.0.2

### New

- Attributes with a `TypeConstraint` of `Component` can now be used when the underlying field type is `GameObject`.
  The syncing converts references from game objects to transforms and vice versa whenever necessary. Note that this
  does not apply to type constraints with a subclass of `Component`.
- Ability to override filter order via the `Order` property.
- `Take`, `TakeLast`, and `TakeWhile` filters.
- `Contains` filter to check that a game object has a component by type.
- `CallbackMethodInfo` struct for a convenient way to get method callbacks for the filters that require it.

### Fixes

- Mismatched references are now handled more correctly.

### Changes

- Made some big internal restructuring changes in the `AutoReferenceAttribute` and `AutoReferenceFilterAttribute`
  classes.
- Filter ordering has been adjusted.
- Some filters are now allowed multiple times on the same field.
- Possibly breaking: `Name` filter now uses case-sensitive matching to mirror Unity's Find methods.
- `ValidationResult` can now be a warning which allows the operation to continue while still logging an output.

### Deprecations

- The constants used previously by filter attributes to specify their `PriorityOrder` have been deprecated.
  The `FilterOrder` static class which contains constants should now be used instead.
- Using a caller type (an external type that's not the behaviour itself) with a non-static method is depreacated.

## 1.0.1

### New

- Some internal functionality was refactored so that it's now exposed to the user. This functionality can be found in
  the new `SceneOperations` and `AssetOperations` classes in the `Teo.AutoReference.Editor` namespace. They provide
  some operations such as syncing scenes and syncing the prefabs in the project.
- Added ability to specify multiple labels in `FindInAssets`

### Fixes

- Fixed some issues with validating existing objects in `FindInAssets`.

## 1.0.1-rc.1

### New

- All bulk syncing operations (i.e. syncing entire scenes or all prefabs) now display a progress bar,
  provided that the bulk operation takes a noticeable time to complete.

Note that syncing all project scenes or all build scenes will take longer than other cases
because these operations need to also load and save scenes that are currently not loaded.

### Improvements

- Progress Bar information is now more accurate.
- Other minor internal improvements

### Fixes

- Syncing build scenes no longer syncs open scenes that aren't included in the build.
- Syncing all prefabs now only selects prefabs that are under the `Assets` folder.

## 1.0.0

First official release!
