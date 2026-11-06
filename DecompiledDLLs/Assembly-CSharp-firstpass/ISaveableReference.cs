using Funlabs;

public interface ISaveableReference : ISaveable
{
	MiniGuid ReferenceId { get; set; }
}
