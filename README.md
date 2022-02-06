
# Native extension for Leopotam [ecslite](https://github.com/Leopotam/ecslite)

An extension for Leopotam ecslite framework with full per entity operations support from Unity Job system side


## Badges
[![MIT License](https://img.shields.io/apm/l/atomic-design-ui.svg?)](https://github.com/tterb/atomic-design-ui/blob/master/LICENSEs)
[![Downloads](https://img.shields.io/github/downloads/odingamesdev/native-ecslite/total.svg)](https://github.com/odingamesdev/native-ecslite/releases)
## Usage/Examples

System example:
```csharp
class ExampleSystem : BaseGameJobSystem<TJob> 
{
  // Or you can provide operations service that has been created in DI container
  public override INativeOperationsService ProvideNativeOperationsService()
    => new NativeOperationsService();

  public override int GetChunkSize(EcsSystems systems)
    => 10;

  public override EcsFilter GetFilter(EcsWorld world) 
  {
    return world.Filter<Component1>().Inc<Component2>().End();
  }

  // Here you should set entities pool
  public override void SetData(EcsSystems systems, ref TJob job) 
  {
    var world = systems.GetWorld();
    var filter = GetFilter()
    
    job.Filter = GetFilterWrapper(filter);

    job.Component1Operations = OperationsService
      .GetReadOnlyOperations<Component1>(systems);

    job.Component2Operations = OperationsService
      .GetReadOnlyOperations<Component2>(systems); 

    job.Component3Operations = OperationsService
      .GetReadWriteOperations<Component3>(systems);
  }
}
```
Components example:
```csharp
struct Component1
{
  int a;
}

struct Component2 
{
  int b;
}

struct Component3 
{
  int c;
}
```
Job example:
```csharp
[BurstCompile]
public struct SummAndWriteJob : IJobParallelFor
{
  public NativeFilterWrapper Filter;

  public ReadOnlyNativeEntityOperations<Component1> Component1Operations;
  public ReadWriteNativeEntityOperations<Component2> Component2Operations;
  public ReadWriteNativeEntityOperations<Component3> Component3Operations;

  public void Execute(int index) 
  {
    var entity = Filter.Entity(index);

    var component1Value = Component1Operations.Get(entity);
    var component2Value = Component2Operations.Get(entity);

    ref var component3Value = ref Component3Operations.Add(entity);

    component3Value = component1Value + component2Value;

    Component2Operations.Del(entity);
  }
}
```
And that's all! All you need is to operate over entity values.
You also can use this mechanism outside ecs system, for example in some kind of service, in this sutuation you need to call `ApplyOperations(EcsSystems systems)` on `NativeOperationsService` by yourself

## Installation

For now to use this extension you need to clone fork of the [ecslite](https://github.com/odingamesdev/ecslite) framework. In future we have plans to merge our project with main Leopotam repo.

## Know problems
1. No AutoReset
2. No internal arrays resizing. Be careful or specify pool arrays size in EcsWorld.Config
3. No way to create entity inside Job
4. Not true entity deletion in Job

## Future plans

1. Entity creation in Job
2. True entity deletion in Job
3. Find ways to resize internal managed arrays in Job that has been wrapped to NativeArray
4. Add AutoReset 
