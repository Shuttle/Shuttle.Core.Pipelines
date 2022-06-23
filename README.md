# Shuttle.Core.Pipelines

```
PM> Install-Package Shuttle.Core.Pipelines
```

Observable event-based pipelines based broadly on pipes and filters.

The `Pipeline` class is defined in the `Shuttle.Core.Infrastructure` package.

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

# Example

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

