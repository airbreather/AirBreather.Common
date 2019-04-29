``` ini

BenchmarkDotNet=v0.11.5, OS=Windows 10.0.17134.753 (1803/April2018Update/Redstone4)
Intel Core i7-6850K CPU 3.60GHz (Skylake), 1 CPU, 12 logical and 6 physical cores
Frequency=3515622 Hz, Resolution=284.4447 ns, Timer=TSC
.NET Core SDK=3.0.100-preview4-011223
  [Host] : .NET Core 2.2.3 (CoreCLR 4.6.27414.05, CoreFX 4.6.27414.05), 64bit RyuJIT
  Clr    : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.3394.0
  Core   : .NET Core 2.2.3 (CoreCLR 4.6.27414.05, CoreFX 4.6.27414.05), 64bit RyuJIT

Server=True  

```
|                  Method |  Job | Runtime |     Mean |    Error |   StdDev | Ratio |        Gen 0 |      Gen 1 | Gen 2 |      Allocated |
|------------------------ |----- |-------- |---------:|---------:|---------:|------:|-------------:|-----------:|------:|---------------:|
|      CountRowsUsingMine |  Clr |     Clr |  2.517 s | 0.0012 s | 0.0012 s |  1.00 |            - |          - |     - |       88.05 KB |
| CountRowsUsingCsvHelper |  Clr |     Clr | 13.632 s | 0.0057 s | 0.0053 s |  5.42 | 1713000.0000 | 30000.0000 |     - | 10530492.67 KB |
|                         |      |         |          |          |          |       |              |            |       |                |
|      CountRowsUsingMine | Core |    Core |  2.110 s | 0.0012 s | 0.0010 s |  1.00 |            - |          - |     - |       80.08 KB |
| CountRowsUsingCsvHelper | Core |    Core | 14.382 s | 0.0054 s | 0.0048 s |  6.82 |   53000.0000 |          - |     - | 10530393.12 KB |
