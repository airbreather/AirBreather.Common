``` ini

BenchmarkDotNet=v0.11.5, OS=Windows 10.0.17134.753 (1803/April2018Update/Redstone4)
Intel Core i7-6850K CPU 3.60GHz (Skylake), 1 CPU, 12 logical and 6 physical cores
Frequency=3515622 Hz, Resolution=284.4447 ns, Timer=TSC
.NET Core SDK=3.0.100-preview4-011223
  [Host]     : .NET Core 2.2.3 (CoreCLR 4.6.27414.05, CoreFX 4.6.27414.05), 64bit RyuJIT
  Job-MCDFDR : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.3394.0
  Job-ZODBUY : .NET Core 2.2.3 (CoreCLR 4.6.27414.05, CoreFX 4.6.27414.05), 64bit RyuJIT
  Job-SBWEIU : .NET CoreRT 1.0.27702.02 @BuiltBy: dlab14-DDVSOWINAGE101 @Branch: master @Commit: a7decabbf1e6b618040b560efcc517d6a050cf1e, 64bit AOT

Server=True  

```
|                  Method | Runtime | csvFile |       Mean |     Error |    StdDev | Ratio | RatioSD |      Gen 0 |    Gen 1 | Gen 2 |   Allocated |
|------------------------ |-------- |-------- |-----------:|----------:|----------:|------:|--------:|-----------:|---------:|------:|------------:|
|      CountRowsUsingMine |     Clr |  mocked |   7.356 ms | 0.0063 ms | 0.0052 ms |  1.00 |    0.00 |          - |        - |     - |        64 B |
| CountRowsUsingCsvHelper |     Clr |  mocked | 164.760 ms | 0.2812 ms | 0.2630 ms | 22.40 |    0.04 | 18000.0000 | 333.3333 |     - | 115764277 B |
|                         |         |         |            |           |           |       |         |            |          |       |             |
|      CountRowsUsingMine |    Core |  mocked |   6.381 ms | 0.0117 ms | 0.0109 ms |  1.00 |    0.00 |          - |        - |     - |        48 B |
| CountRowsUsingCsvHelper |    Core |  mocked | 177.825 ms | 0.3843 ms | 0.3595 ms | 27.87 |    0.08 |   333.3333 |        - |     - | 115757736 B |
|                         |         |         |            |           |           |       |         |            |          |       |             |
|      CountRowsUsingMine |  CoreRT |  mocked |   6.557 ms | 0.0126 ms | 0.0118 ms |  1.00 |    0.00 |          - |        - |     - |        48 B |
| CountRowsUsingCsvHelper |  CoreRT |  mocked | 133.498 ms | 0.1710 ms | 0.1428 ms | 20.35 |    0.05 |   500.0000 |        - |     - | 114615182 B |
