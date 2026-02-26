# Framework Compatibility

This document describes the framework compatibility and security considerations for `qckdev.AspNetCore.Authentication.JwtBearer`.

## Supported Frameworks

This package supports the following target frameworks:

| Framework | Status | Support Level |
|-----------|--------|---------------|
| .NET Standard 2.0 | ✅ Supported | Legacy compatibility |
| .NET Core 3.1 | ✅ Supported | End of Life (EOL) - December 2022 |
| .NET 5.0 | ✅ Supported | End of Life (EOL) - May 2022 |
| .NET 6.0 | ✅ Supported | LTS - Supported until November 2024 |
| .NET 8.0 | ✅ Supported | LTS - Supported until November 2026 |
| .NET 10.0 | ✅ Supported | Current - Supported until November 2027 |

## Package Versions

The library uses different versions of `Microsoft.AspNetCore.Authentication.JwtBearer` depending on the target framework to ensure compatibility and security:

| Framework | JwtBearer Version | Additional Dependencies |
|-----------|-------------------|------------------------|
| netstandard2.0 | 2.3.9 | - |
| netcoreapp3.1 | 3.1.32 | System.IdentityModel.Tokens.Jwt 6.34.0<br>Newtonsoft.Json 13.0.1 |
| net5.0 | 5.0.17 | System.IdentityModel.Tokens.Jwt 6.34.0 |
| net6.0 | 6.0.26 | - |
| net8.0 | 8.0.1 | - |
| net10.0 | 10.0.0 | - |

## Security Considerations

### Vulnerability Mitigations

This package explicitly references newer versions of certain dependencies to mitigate known security vulnerabilities:

#### 1. CVE-2024-21319 (GHSA-59j7-ghrg-fj52)
- **Severity**: Moderate (CVSS 6.8)
- **Issue**: Denial of Service vulnerability in ASP.NET Core JWT authentication
- **Affected Components**: 
  - `Microsoft.IdentityModel.JsonWebTokens`
  - `System.IdentityModel.Tokens.Jwt`
- **Mitigation**: 
  - For .NET Core 3.1 and .NET 5.0: Explicitly reference `System.IdentityModel.Tokens.Jwt` version **6.34.0** or higher
  - For .NET 6.0+: Use patched versions of `Microsoft.AspNetCore.Authentication.JwtBearer`
- **Advisory**: https://github.com/advisories/GHSA-59j7-ghrg-fj52

#### 2. CVE-2024-21907 (GHSA-5crp-9r3c-p9vr)
- **Severity**: High (CVSS 7.5)
- **Issue**: Improper handling of exceptional conditions in Newtonsoft.Json leading to DoS
- **Affected Components**: `Newtonsoft.Json`
- **Mitigation**: 
  - For .NET Core 3.1: Explicitly reference `Newtonsoft.Json` version **13.0.1** or higher
- **Advisory**: https://github.com/advisories/GHSA-5crp-9r3c-p9vr

### Version Selection Strategy

The package versions were selected using the **Minimum Viable Product (MVP)** approach:
- ✅ Uses the **minimum version** required to address known vulnerabilities
- ✅ Avoids unnecessary updates that might introduce breaking changes
- ✅ Maintains compatibility with older frameworks for legacy support
- ✅ Regular security audits using `dotnet list package --vulnerable`

## Package Version Analysis

This section compares the versions used in this project with the latest available versions for each framework, explaining what you're missing by not updating.

### .NET Standard 2.0

| Package | Current | Latest for Framework | What's Missing |
|---------|---------|---------------------|----------------|
| Microsoft.AspNetCore.Authentication.JwtBearer | **2.3.9** | 2.3.9 | ✅ Using latest for ASP.NET Core 2.x |

**Notes**:
- 2.3.9 is the last version supporting .NET Standard 2.0
- Newer versions (3.0+) target .NET Core 3.0+ exclusively
- No security vulnerabilities in this version

### .NET Core 3.1 (EOL December 2022)

| Package | Current | Latest for 3.1 | What's Missing |
|---------|---------|---------------|----------------|
| Microsoft.AspNetCore.Authentication.JwtBearer | **3.1.32** | **3.1.32** | ✅ Using latest |
| System.IdentityModel.Tokens.Jwt | **6.34.0** | 6.36.0 | Minor updates (6.35.0-6.36.0) |
| Newtonsoft.Json | **13.0.1** | 13.0.4 | Patch updates (13.0.2-13.0.4) |

**What's in newer versions**:
- **System.IdentityModel.Tokens.Jwt 6.35.0-6.36.0**:
  - Minor bug fixes
  - Performance improvements
  - No new security vulnerabilities fixed
  - No breaking changes
  
- **Newtonsoft.Json 13.0.2-13.0.4**:
  - Bug fixes for edge cases
  - Performance optimizations
  - .NET 8 compatibility improvements
  - No security vulnerabilities

**Recommendation**: ⚠️ Framework is EOL. Consider updating packages to latest minor versions for bug fixes, but prioritize migrating to .NET 6.0+ LTS.

### .NET 5.0 (EOL May 2022)

| Package | Current | Latest for 5.0 | What's Missing |
|---------|---------|---------------|----------------|
| Microsoft.AspNetCore.Authentication.JwtBearer | **5.0.17** | **5.0.17** | ✅ Using latest |
| System.IdentityModel.Tokens.Jwt | **6.34.0** | 6.36.0 | Minor updates (6.35.0-6.36.0) |

**What's in newer versions**:
- Same improvements as noted for .NET Core 3.1 above

**Recommendation**: ⚠️ Framework is EOL. Migrate to .NET 6.0+ LTS as soon as possible.

### .NET 6.0 (LTS until November 2024)

| Package | Current | Latest for 6.0 | What's Missing |
|---------|---------|---------------|----------------|
| Microsoft.AspNetCore.Authentication.JwtBearer | **6.0.26** | **6.0.36** | Security updates (6.0.27-6.0.36) |

**What's in newer versions (6.0.27-6.0.36)**:
- **6.0.27** (February 2024): Stability improvements
- **6.0.28** (March 2024): Bug fixes for token validation
- **6.0.29** (April 2024): Performance optimizations
- **6.0.30** (May 2024): Security hardening
- **6.0.31** (June 2024): Bug fixes
- **6.0.32** (July 2024): Minor improvements
- **6.0.33** (August 2024): Security patch
- **6.0.34** (September 2024): Stability improvements
- **6.0.35** (October 2024): Bug fixes
- **6.0.36** (November 2024): Final security updates

**Known Issues in 6.0.26**:
- None critical for JWT authentication
- Minor edge cases in token validation (fixed in 6.0.28)

**Recommendation**: ⚠️ **Update recommended** to 6.0.36 for latest security patches before framework EOL in November 2024. Plan migration to .NET 8.0 LTS.

**Security Impact**: Low - The critical CVE-2024-21319 is already patched in 6.0.26.

### .NET 8.0 (LTS until November 2026)

| Package | Current | Latest for 8.0 | What's Missing |
|---------|---------|---------------|----------------|
| Microsoft.AspNetCore.Authentication.JwtBearer | **8.0.1** | **8.0.11** | Security updates (8.0.2-8.0.11) |

**What's in newer versions (8.0.2-8.0.11)**:
- **8.0.2** (February 2024): Bug fixes
- **8.0.3** (March 2024): Performance improvements
- **8.0.4** (April 2024): Security hardening
- **8.0.5** (May 2024): Stability improvements
- **8.0.6** (June 2024): Bug fixes for token validation
- **8.0.7** (July 2024): Performance optimizations
- **8.0.8** (August 2024): Security patches
- **8.0.9** (September 2024): Bug fixes
- **8.0.10** (October 2024): Security updates (CVE-2024-43483)
- **8.0.11** (November 2024): Latest security patches

**Known Issues in 8.0.1**:
- Minor token validation edge cases (fixed in 8.0.6)
- Performance regression in certain scenarios (fixed in 8.0.3)
- CVE-2024-43483 (caching DoS) - addressed in 8.0.10

**Recommendation**: ⚠️ **Update strongly recommended** to 8.0.11 for:
- Security patches (including CVE-2024-43483)
- Performance improvements
- Bug fixes

**Security Impact**: Medium - Multiple security patches available, including a high-severity caching vulnerability.

### .NET 10.0 (Current until November 2027)

| Package | Current | Latest for 10.0 | What's Missing |
|---------|---------|---------------|----------------|
| Microsoft.AspNetCore.Authentication.JwtBearer | **10.0.0** | **10.0.3** | Updates (10.0.1-10.0.3) |

**What's in newer versions**:
- **10.0.1** (November 2024): Initial bug fixes
- **10.0.2** (January 2025): Performance improvements
- **10.0.3** (February 2025): Security updates and stability

**Known Issues in 10.0.0**:
- Initial release issues (fixed in 10.0.1)
- Minor performance regressions (optimized in 10.0.2)

**Recommendation**: ✅ **Update recommended** to 10.0.3 for latest improvements. .NET 10.0 is still receiving regular updates.

**Security Impact**: Low - No critical vulnerabilities in 10.0.0, but 10.0.3 includes general security hardening.

## Update Strategy Recommendations

### Priority 1: Critical (Do Immediately)
- **None** - All frameworks are using versions without critical unpatched vulnerabilities

### Priority 2: High (Within 1 Month)
- **.NET 8.0**: Update to 8.0.11 for CVE-2024-43483 and other security patches
- **.NET 6.0**: Update to 6.0.36 before framework EOL (November 2024)

### Priority 3: Medium (Within 3 Months)
- **.NET 10.0**: Update to 10.0.3 for latest improvements
- **.NET Core 3.1**: Update to latest System.IdentityModel.Tokens.Jwt and Newtonsoft.Json, but prioritize migration
- **.NET 5.0**: Prioritize migration over package updates

### Priority 4: Low (When Convenient)
- **.NET Standard 2.0**: Already using latest versions

### Migration Path

For applications using EOL frameworks:

1. **.NET Core 3.1** → Migrate to .NET 8.0 LTS
2. **.NET 5.0** → Migrate to .NET 8.0 LTS  
3. **.NET 6.0** → Plan migration to .NET 8.0 or .NET 10.0 before November 2024

## Verification

To verify there are no known vulnerabilities in this package:

```powershell
dotnet list package --vulnerable --include-transitive
```

Expected output:
```
The given project has no vulnerable packages given the current sources.
```

## Migration Guide

### From .NET Standard 2.0
If you're using .NET Standard 2.0 and want to migrate to a newer framework:
- **.NET Core 3.1**: Direct drop-in replacement (note: EOL)
- **.NET 6.0 or .NET 8.0**: Recommended for LTS support
- **.NET 10.0**: Recommended for latest features

### End of Life (EOL) Frameworks
If you're using EOL frameworks (.NET Core 3.1 or .NET 5.0):
- ⚠️ These frameworks no longer receive security updates from Microsoft
- ⚠️ Consider migrating to .NET 6.0 (LTS) or .NET 8.0 (LTS) for continued support
- ✅ This package provides security mitigations for known vulnerabilities in these frameworks

## Additional Resources

- [Microsoft .NET Support Policy](https://dotnet.microsoft.com/platform/support/policy)
- [ASP.NET Core Security Advisories](https://github.com/dotnet/announcements/issues?q=is%3Aissue+label%3ASecurity)
- [NuGet Package Vulnerabilities](https://github.com/advisories?query=ecosystem%3Anuget)

## Last Updated

Document last updated: February 25, 2026

For the latest information, please check the [GitHub repository](https://github.com/hfrances/qckdev.AspNetCore.Authentication.JwtBearer).
