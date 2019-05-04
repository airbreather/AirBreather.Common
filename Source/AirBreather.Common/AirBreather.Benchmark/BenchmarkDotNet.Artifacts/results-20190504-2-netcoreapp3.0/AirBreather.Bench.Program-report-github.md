``` ini

BenchmarkDotNet=v0.11.5, OS=Windows 10.0.17134.753 (1803/April2018Update/Redstone4)
Intel Core i7-6850K CPU 3.60GHz (Skylake), 1 CPU, 12 logical and 6 physical cores
Frequency=3515622 Hz, Resolution=284.4447 ns, Timer=TSC
.NET Core SDK=3.0.100-preview4-011223
  [Host]     : .NET Core 3.0.0-preview4-27615-11 (CoreCLR 4.6.27615.73, CoreFX 4.700.19.21213), 64bit RyuJIT
  Job-UIOCLB : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.3394.0
  Job-RCBFPW : .NET Core 3.0.0-preview4-27615-11 (CoreCLR 4.6.27615.73, CoreFX 4.700.19.21213), 64bit RyuJIT
  Job-LFZQHY : .NET CoreRT 1.0.27704.01 @BuiltBy: dlab14-DDVSOWINAGE101 @Branch: master @Commit: 64fca2f1b1b1bf6d5c1bbc0df3b9c55eddd63d37, 64bit AOT

Server=True  

```
|                  Method | Runtime | csvFile |       Mean |     Error |    StdDev | Ratio | RatioSD |      Gen 0 |    Gen 1 | Gen 2 |   Allocated |
|------------------------ |-------- |-------- |-----------:|----------:|----------:|------:|--------:|-----------:|---------:|------:|------------:|
|      CountRowsUsingMine |     Clr |  mocked |  11.723 ms | 0.0393 ms | 0.0348 ms |  1.00 |    0.00 |          - |        - |     - |       256 B |
| CountRowsUsingCsvHelper |     Clr |  mocked | 166.222 ms | 0.9912 ms | 0.9272 ms | 14.18 |    0.10 | 18000.0000 | 333.3333 |     - | 115764283 B |
|                         |         |         |            |           |           |       |         |            |          |       |             |
|      CountRowsUsingMine |    Core |  mocked |  10.342 ms | 0.0213 ms | 0.0189 ms |  1.00 |    0.00 |          - |        - |     - |       216 B |
| CountRowsUsingCsvHelper |    Core |  mocked | 159.995 ms | 0.3022 ms | 0.2827 ms | 15.47 |    0.05 |  1666.6667 |        - |     - | 114613744 B |
|                         |         |         |            |           |           |       |         |            |          |       |             |
|      CountRowsUsingMine |  CoreRT |  mocked |   8.771 ms | 0.0462 ms | 0.0432 ms |  1.00 |    0.00 |          - |        - |     - |       208 B |
| CountRowsUsingCsvHelper |  CoreRT |  mocked | 127.769 ms | 0.2357 ms | 0.2089 ms | 14.56 |    0.08 |   500.0000 |        - |     - | 114615166 B |
