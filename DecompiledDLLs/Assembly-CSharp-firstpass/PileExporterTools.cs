using System.Collections;
using UnityEngine;

public class PileExporterTools : MonoBehaviour
{
	public GameObject Template;

	public GameObject TemplateContainer;

	public GameObject SpawnPoint;

	public GameObject SpawnContainer;

	public BoxCollider BoundsL;

	public BoxCollider BoundsR;

	public BoxCollider BoundsU;

	public BoxCollider BoundsD;

	public float BoundsSize = 1f;

	public int Qty;

	public float SpawnRate;

	public Vector3 Rotation;

	public bool RandomRotation;

	private void Awake()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		((Component)BoundsL).transform.localScale = Vector3.one * BoundsSize;
		((Component)BoundsR).transform.localScale = Vector3.one * BoundsSize;
		((Component)BoundsU).transform.localScale = Vector3.one * BoundsSize;
		((Component)BoundsD).transform.localScale = Vector3.one * BoundsSize;
		((Component)BoundsL).transform.position = new Vector3(0f - BoundsSize, BoundsSize / 2f, 0f);
		((Component)BoundsR).transform.position = new Vector3(BoundsSize, BoundsSize / 2f, 0f);
		((Component)BoundsU).transform.position = new Vector3(0f, BoundsSize / 2f, 0f - BoundsSize);
		((Component)BoundsD).transform.position = new Vector3(0f, BoundsSize / 2f, BoundsSize);
	}

	private void OnDestroy()
	{
		for (int i = 0; i < TemplateContainer.transform.childCount; i++)
		{
			Object.Destroy((Object)(object)((Component)TemplateContainer.transform.GetChild(i)).gameObject);
		}
	}

	private void PrepTemplate()
	{
		GameObject val = Object.Instantiate<GameObject>(Template, TemplateContainer.transform);
		Mesh mesh = val.GetComponentInChildren<MeshFilter>().mesh;
		Component[] components = val.GetComponents<Component>();
		foreach (Component val2 in components)
		{
			try
			{
				Object.DestroyImmediate((Object)(object)val2);
			}
			catch
			{
			}
		}
		MeshCollider val3 = val.AddComponent<MeshCollider>();
		val3.sharedMesh = mesh;
		val3.convex = true;
		Rigidbody val4 = val.AddComponent<Rigidbody>();
		val4.useGravity = true;
		val4.isKinematic = false;
	}

	private void Spawn()
	{
		if (Application.isPlaying)
		{
			((MonoBehaviour)this).StartCoroutine(Spawn_Sequence());
		}
	}

	private void DestroyInstances()
	{
		for (int i = 0; i < SpawnContainer.transform.childCount; i++)
		{
			Object.Destroy((Object)(object)((Component)SpawnContainer.transform.GetChild(i)).gameObject);
		}
	}

	private IEnumerator Spawn_Sequence()
	{
		for (int i = 0; i < Qty; i++)
		{
			GameObject val = Object.Instantiate<GameObject>(((Component)TemplateContainer.transform.GetChild(0)).gameObject);
			val.transform.parent = SpawnContainer.transform;
			val.transform.position = SpawnPoint.transform.position;
			Transform transform = val.transform;
			transform.rotation *= Quaternion.Euler(Rotation);
			if (RandomRotation)
			{
				val.transform.rotation = Random.rotation;
			}
			yield return (object)new WaitForSeconds(SpawnRate);
		}
	}

	private void Export()
	{
	}
}
