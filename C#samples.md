```csharp
// using Polly policy https://github.com/NVIDIA/clara-dicom-adapter/blob/main/src/Server/Services/Scp/ApplicationEntityHandler.cs
Policy.Handle<Exception>()
                .WaitAndRetry(3,
                (retryAttempt) =>
                {
                    return retryAttempt == 1 ? TimeSpan.FromMilliseconds(250) : TimeSpan.FromMilliseconds(500);
                },
                    (exception, retryCount, context) =>
                {
                    _logger.Log(LogLevel.Error, "Failed to save instance, retry count={retryCount}: {exception}", retryCount, exception);
                })
                .Execute(() =>
                {
                    if (ShouldSaveInstance(instanceStorage))
                    {
                        _logger.Log(LogLevel.Information, "Saving {path}.", instanceStorage.InstanceStorageFullPath);
                        _fileSystem.Directory.CreateDirectory(instanceStorage.SeriesStoragePath);
                        _dicomToolkit.Save(request.File, instanceStorage.InstanceStorageFullPath);
                        _logger.Log(LogLevel.Debug, "Instance saved successfully.");
                        _instanceStoredNotificationService.NewInstanceStored(instanceStorage);
                        _logger.Log(LogLevel.Information, "Instance stored and notified successfully.");
                    }
                });
```
