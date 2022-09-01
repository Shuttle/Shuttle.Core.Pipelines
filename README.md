# Shuttle.Core.Pipelines

```
PM> Install-Package Shuttle.Core.Pipelines
```

Observable event-based pipelines based broadly on pipes and filters.

## Configuration

In order to more easily make use of pipelines an implementation of the `IPipelineFactory` should be used.  The following will register the `PipelineFactory` implementation:

```c#
services.AddPipelineProcessing(builder => {
    builder.AddAssembly(assembly);
});
```

This will register the `IPipelineFactory` and, using the builder, add all `IPipeline` and `IPipelineObserver` implementations as `Transient`.  The pipeline instances are re-used as they are kept in a pool.

Since pipelines are quite frequently extended by adding observers a *module* may be added that adds the relevant observers to a pipeline on creation:

```c#
services.AddPipelineModule<T>();
services.AddPipelineModule(type);
```

The way this is accomplished is having a module such as the following:

```c#
public class SomeModule
{
    private readonly Type _pipelineType = typeof(InterestingPipeline);

    public SomeModule(IPipelineFactory pipelineFactory)
    {
        Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));

        pipelineFactory.PipelineCreated += PipelineCreated;
    }

    private void PipelineCreated(object sender, PipelineEventArgs e)
    {
        var pipelineType = ;

        if (e.Pipeline.GetType() != _pipelineType
        {
            return;
        }

        e.Pipeline.RegisterObserver(new SomeObserver());
    }
}
```

The above module could be added to the `IServiceCollection` as follows:

```c#
services.AddPipelineModule<SomeModule>();
```

## Overview

A `Pipeline` is a variation of the pipes and filters pattern and consists of 1 or more stages that each contain one or more events.  When the pipeline is executed each event in each stage is raised in the order that they were registered.  One or more observers should be registered to handle the relevant event(s).

Each `Pipeline` always has its own state that is simply a name/value pair with some convenience methods to get and set/replace values.  The `State` class will use the full type name of the object as a key should none be specified:

``` c#
var state = new State();
var list = new List<string> {"item-1"};

state.Add(list); // key = System.Collections.Generic.List`1[[System.String...]]
state.Add("my-key", "my-key-value");

Console.WriteLine(state.Get<List<string>>()[0]);
Console.Write(state.Get<string>("my-key"));
```

## Example

Events should derive from `PipelineEvent`.

We will use the following events:

``` c#
public class OnAddCharacterA : PipelineEvent
{
}

public class OnAddCharacter : PipelineEvent
{
	public char Character { get; private set; }

	public OnAddCharacter(char character)
	{
		Character = character;
	}
}
```

The `OnAddCharacterA` event represents a very explicit event whereas with the `OnAddCharacter` event one can specify some data.  In this case the character to add.

In order for the pipeline to process the events we will have to define one or more observers to handle the events.  We will define only one for this sample but we could very easily add another that will handle one or more of the same, or other, events:

``` c#
    public class CharacterPipelineObserver : 
        IPipelineObserver<OnAddCharacterA>,
        IPipelineObserver<OnAddCharacter>
    {
        public void Execute(OnAddCharacterA pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var value = state.Get<string>("value");

            value = string.Format("{0}-A", value);

            state.Replace("value", value);
        }

        public void Execute(OnAddCharacter pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var value = state.Get<string>("value");

            value = string.Format("{0}-{1}", value, pipelineEvent.Character);

            state.Replace("value", value);
        }
    }
```

Next we will define the pipeline itself:

``` c#
var pipeline = new Pipeline();

pipeline.RegisterStage("process")
	.WithEvent<OnAddCharacterA>()
	.WithEvent(new OnAddCharacter('Z'));

pipeline.RegisterObserver(new CharacterPipelineObserver());

pipeline.State.Add("value", "start");
pipeline.Execute();

Console.WriteLine(pipeline.State.Get<string>("value")); // outputs start-A-Z
```

We can now execute this pipeline with predictable results.

Pipelines afford us the ability to better decouple functionality.  This means that we could re-use the same observer in multiple pipelines enabling us to compose functionality at a more granular level.

