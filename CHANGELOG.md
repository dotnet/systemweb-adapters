# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [Preview 2]

### Added
- Ability to log (or throw) unknown session keys. Previously, this would silently fail with a difficult to diagnose issue.

### Fixed
- Saving remote session state would sometimes fail due to an uncaught exception

### Changed
- Changed default remote session app endpoint to `/systemweb-adapters/session`

### Removed
- .NET Core 3.1 support

## [Preview 1] - 2022-05-20

### Added
- Initial support for `System.Web.HttpContext` adapters
- Initial support for session management
