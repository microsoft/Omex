# How to use the Gating library

In your code you will define `Gates` that define access control.
A `Gate` can then be used to create a `GatedAction` or a `GatedFunc`.
Those are instances of the `GatedCode` class. `GatedCode` can be invoked in a `GateContext`.
To create a `GateContext` you will need a `GatedRequest` containing user information
and an instance of `IMachineInformation` for current machine environment information.

Usually it will look like this:

1. Service receives a user request
2. A `GatedRequest` is constructed for the user request
3. A `GateContext` is constructed with the `GatedRequest`
4. A gated code path is accessed
5. The `GatedCode` is invoked in the `GateContext`
6. `GateContext` decides if `Gate` conditions are satisfied

_From the [Gating.Example](https://github.com/microsoft/Omex/tree/master/src/Gating.Example):_

```cs
Gates gates = new Gates(GateDataSetLoader);

IGatedRequest gatedRequest = new SampleGatedRequest();
IGateContext gateContext = new GateContext(gatedRequest, machineInformation, null);
gateContext.PerformAction(
    new GatedAction(
        gates.GetGate("sample_allowed_gate"),
        () => Console.WriteLine("SampleAction has been called")));
```

## Usage scenarios

For each gate defined in the XML configuration you'd create a class containing
the appropriate definitions. This can be done using the [`GateGen` tool](https://github.com/microsoft/Omex/tree/master/src/CodeGenerators/GateGen).

Any class whose behaviour depends on gates will take `GateContext` in its constructor.
Then you can use `GateExtensions.PerformAction(gate, Action)` on the context like an If statement.
`PerformAction` will perform at most one action - the first one whose gate is applicable.
By adding a `GatedAction` without a gate, you create a _default_ action, like an Else branch.

```cs
public class PointsAdder
{
    private readonly IGateContext m_gateContext;
    private readonly User m_user;

    public PointsAdder(IGateContext gateContext, User user)
    {
        m_gateContext = gateContext;
        m_user = user;
    }

    public void AddPoints()
    {
        // If (user is SuperUser)
        //    add them 10 points
        // else
        //    add them 5 points
        m_gateContext.PerformAction(
            new GatedAction(
                Gates.SuperUser,
                () => m_user.Points += 10
            ),
            new GatedAction(
                () => m_user.Points += 5
            )
        );
    }
}
```

Another approach is to use the `PerformEachAction` method which will invoke code for every applicable `GateAction`, not just the first.

```cs
public List<Item> GetItems()
{
    var items = new List<Item>();
    m_gateContext.PerformEachAction(
        new GatedAction(
            Gates.Admininstrators,
            () => AddAdminItems(items)
        ),
        new GatedAction(
            () => AddGeneralItems(items)
        )
    );
}
```

Very similar will be the use of `PerformFunction` and `GatedFunc`, but with a return value.

## Required implementations

When using the Gating library, you'll need to implement your own `GatedRequest` class inheriting from
`AbstractGatedRequest` or just implementing `IGatedRequest`.

You can implement `IMachineInformation` interface from `Microsoft.Omex.System.Diagnostics` namespace
to provide more information about the machine your app is running on.

You can also implement `IGateSettings` interface to override applicability of gates.

## Combining Gates

You can combine gates by using `GateCombination` subclasses

* `GatesAll` - applicable if all combined gates are applicable
* `GatesAny` - applicable if at least one of the combined gates is applicable
* `GatesNone` - applicable if none of the combined gates are applicable

Gates can also be setup in a hierarchical structure. Every gate has a property `ParentGate` and
restrictions which are applied on a parent gate apply to all of its child gates. So, in effect
restrictions on a child gate are an intersection of the restrictions between its parent and itself.
