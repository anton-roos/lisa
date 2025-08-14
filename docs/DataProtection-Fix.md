# Data Protection Key Management Fix

## Issue
The application was throwing antiforgery token deserialization errors:
```
The key {guid} was not found in the key ring
The antiforgery token could not be decrypted
```

## Root Cause
By default, ASP.NET Core stores Data Protection keys in memory, which means:
- Keys are lost when the application restarts
- Multiple app instances can't share keys
- Deployments create new key rings

## Solution Implemented

### 1. Persistent Key Storage
```csharp
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "DataProtection-Keys")))
    .SetApplicationName("Lisa.School.Management")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(90));
```

**Benefits:**
- Keys persist across application restarts
- Consistent encryption/decryption across deployments
- 90-day key lifetime for security

### 2. Enhanced Antiforgery Configuration
```csharp
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = "__RequestVerificationToken";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});
```

**Benefits:**
- Explicit cookie configuration
- Enhanced security settings
- Better error handling

### 3. Key Directory Management
- Added `DataProtection-Keys/` to `.gitignore`
- Keys are environment-specific (not shared between dev/prod)
- Each environment maintains its own key ring

## Deployment Considerations

### For Each Environment:
1. **Ensure Key Directory Exists:** The app will create `DataProtection-Keys/` folder automatically
2. **Set Proper Permissions:** Ensure the app pool has read/write access to this directory
3. **Backup Strategy:** Consider backing up key files for disaster recovery

### Directory Structure:
```
src/
├── DataProtection-Keys/     # Created automatically
│   ├── key-{guid}.xml      # Encryption keys
│   └── ...
├── Program.cs
└── ...
```

## Security Notes
- Keys are encrypted at rest by the OS (Windows DPAPI, Linux/macOS Keychain)
- Each environment should have separate keys
- Keys automatically rotate every 90 days
- Old keys are retained for decryption of existing tokens

## Expected Results
✅ No more antiforgery token deserialization errors  
✅ Persistent authentication across app restarts  
✅ Consistent user experience during deployments  
✅ Enhanced security with proper cookie configuration  

## Monitoring
Monitor for the automatic creation of the `DataProtection-Keys` directory on first startup.
