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
|                  Method |  Job | Runtime |      Mean |     Error |    StdDev | Ratio | RatioSD |      Gen 0 |     Gen 1 | Gen 2 |    Allocated |
|------------------------ |----- |-------- |----------:|----------:|----------:|------:|--------:|-----------:|----------:|------:|-------------:|
|      CountRowsUsingMine |  Clr |     Clr | 108.73 ms | 0.1123 ms | 0.1050 ms |  1.00 |    0.00 |          - |         - |     - |     81.65 KB |
| CountRowsUsingCsvHelper |  Clr |     Clr | 568.26 ms | 1.0035 ms | 0.8895 ms |  5.23 |    0.01 | 71000.0000 | 1000.0000 |     - | 443640.71 KB |
|                         |      |         |           |           |           |       |         |            |           |       |              |
|      CountRowsUsingMine | Core |    Core |  90.39 ms | 0.1871 ms | 0.1659 ms |  1.00 |    0.00 |          - |         - |     - |     80.08 KB |
| CountRowsUsingCsvHelper | Core |    Core | 601.87 ms | 0.9943 ms | 0.9301 ms |  6.66 |    0.02 |  2000.0000 |         - |     - | 443633.32 KB |
