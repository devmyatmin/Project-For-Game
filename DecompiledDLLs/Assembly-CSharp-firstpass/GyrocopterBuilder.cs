using System.Linq;
using Beam.Crafting;
using UnityEngine;

public class GyrocopterBuilder : MonoBehaviour
{
	public Constructing BasePrefab;

	public Constructing CockpitPrefab;

	public Constructing MotorPrefab;

	public Constructing RotorsPrefab;

	public Constructing SeatPrefab;

	private void Start()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		Constructing constructing = Object.Instantiate<Constructing>(BasePrefab);
		constructing.Place();
		Constructing constructing2 = Object.Instantiate<Constructing>(CockpitPrefab);
		constructing2.SnappedConnector = constructing.MyConnectors.FirstOrDefault((Connector connector) => ((Object)connector).name == "Connector_Cockpit");
		constructing2.transform.position = ((Component)constructing2.SnappedConnector).transform.position;
		constructing2.transform.rotation = ((Component)constructing2.SnappedConnector).transform.rotation;
		constructing2.Place();
		Constructing constructing3 = Object.Instantiate<Constructing>(MotorPrefab);
		constructing3.SnappedConnector = constructing.MyConnectors.FirstOrDefault((Connector connector) => ((Object)connector).name == "Connector_Motor");
		constructing3.transform.position = ((Component)constructing3.SnappedConnector).transform.position;
		constructing3.transform.rotation = ((Component)constructing3.SnappedConnector).transform.rotation;
		constructing3.Place();
		Constructing constructing4 = Object.Instantiate<Constructing>(RotorsPrefab);
		constructing4.SnappedConnector = constructing.MyConnectors.FirstOrDefault((Connector connector) => ((Object)connector).name == "Connector_Rotor");
		constructing4.transform.position = ((Component)constructing4.SnappedConnector).transform.position;
		constructing4.transform.rotation = ((Component)constructing4.SnappedConnector).transform.rotation;
		constructing4.Place();
		Constructing constructing5 = Object.Instantiate<Constructing>(SeatPrefab);
		constructing5.SnappedConnector = constructing.MyConnectors.FirstOrDefault((Connector connector) => ((Object)connector).name == "Connector_Seat");
		constructing5.transform.position = ((Component)constructing5.SnappedConnector).transform.position;
		constructing5.transform.rotation = ((Component)constructing5.SnappedConnector).transform.rotation;
		constructing5.Place();
	}
}
