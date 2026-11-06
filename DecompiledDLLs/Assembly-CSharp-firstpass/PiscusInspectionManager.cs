using System.Collections.Generic;
using Beam;

public class PiscusInspectionManager : Singleton<PiscusInspectionManager>
{
	public enum PiscusType
	{
		ALL,
		SHARK_TIGER,
		SHARK_WHITE,
		SHARK_REEF,
		MARLIN
	}

	public List<Piscus_Creature> piscuses = new List<Piscus_Creature>();
}
