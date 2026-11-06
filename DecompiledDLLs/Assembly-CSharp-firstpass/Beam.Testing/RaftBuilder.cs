using System.Linq;
using Beam.Crafting;
using UnityEngine;

namespace Beam.Testing;

public class RaftBuilder : MonoBehaviour
{
	public Construction_RAFT BasePrefab;

	public Construction_RAFT FloorPrefab;

	public Constructing_SAIL SailPrefab;

	public Constructing_VEHICLE RudderPrefab;

	public Constructing_BOAT_MOTOR MotorPrefab;

	public Constructing_ANCHOR AnchorPrefab;

	private void Start()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		Construction_RAFT construction_RAFT = Object.Instantiate<Construction_RAFT>(BasePrefab, ((Component)this).transform.position, Quaternion.identity);
		construction_RAFT.Place();
		Construction_RAFT construction_RAFT2 = Object.Instantiate<Construction_RAFT>(FloorPrefab);
		construction_RAFT2.SnappedConnector = construction_RAFT.MyConnectors.FirstOrDefault((Connector connector) => ((Object)connector).name == "FloorConnector");
		construction_RAFT2.transform.position = ((Component)construction_RAFT2.SnappedConnector).transform.position;
		construction_RAFT2.transform.rotation = ((Component)construction_RAFT2.SnappedConnector).transform.rotation;
		construction_RAFT2.Place();
		Constructing_SAIL constructing_SAIL = Object.Instantiate<Constructing_SAIL>(SailPrefab);
		constructing_SAIL.SnappedConnector = construction_RAFT2.MyConnectors.FirstOrDefault((Connector connector) => ((Object)connector).name == "VehicleConnector_Mast");
		constructing_SAIL.transform.position = ((Component)constructing_SAIL.SnappedConnector).transform.position;
		constructing_SAIL.transform.rotation = ((Component)constructing_SAIL.SnappedConnector).transform.rotation;
		constructing_SAIL.Place();
		Constructing_BOAT_MOTOR constructing_BOAT_MOTOR = Object.Instantiate<Constructing_BOAT_MOTOR>(MotorPrefab);
		constructing_BOAT_MOTOR.SnappedConnector = construction_RAFT2.MyConnectors.FirstOrDefault((Connector connector) => ((Object)connector).name == "VehicleConnector_Motor_Bottom");
		constructing_BOAT_MOTOR.transform.position = ((Component)constructing_BOAT_MOTOR.SnappedConnector).transform.position;
		constructing_BOAT_MOTOR.transform.rotation = ((Component)constructing_BOAT_MOTOR.SnappedConnector).transform.rotation;
		constructing_BOAT_MOTOR.Place();
		Constructing_ANCHOR constructing_ANCHOR = Object.Instantiate<Constructing_ANCHOR>(AnchorPrefab);
		constructing_ANCHOR.SnappedConnector = construction_RAFT2.MyConnectors.FirstOrDefault((Connector connector) => ((Object)connector).name == "VehicleConnector_Motor_Top");
		constructing_ANCHOR.transform.position = ((Component)constructing_ANCHOR.SnappedConnector).transform.position;
		constructing_ANCHOR.transform.rotation = ((Component)constructing_ANCHOR.SnappedConnector).transform.rotation;
		constructing_ANCHOR.Place();
	}
}
