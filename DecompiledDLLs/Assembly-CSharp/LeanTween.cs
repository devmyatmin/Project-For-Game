using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LeanTween : MonoBehaviour
{
	public static bool throwErrors = true;

	public static float tau = (float)Math.PI * 2f;

	private static LTDescrImpl[] tweens;

	private static int[] tweensFinished;

	private static LTDescrImpl tween;

	private static int tweenMaxSearch = -1;

	private static int maxTweens = 400;

	private static int frameRendered = -1;

	private static GameObject _tweenEmpty;

	private static float dtEstimated = -1f;

	public static float dtManual;

	private static float dt;

	private static float dtActual;

	private static int i;

	private static int j;

	private static int finishedCnt;

	private static AnimationCurve punch = new AnimationCurve(new Keyframe[9]
	{
		new Keyframe(0f, 0f),
		new Keyframe(0.112586f, 0.9976035f),
		new Keyframe(0.3120486f, -0.1720615f),
		new Keyframe(0.4316337f, 0.07030682f),
		new Keyframe(0.5524869f, -0.03141804f),
		new Keyframe(0.6549395f, 0.003909959f),
		new Keyframe(0.770987f, -0.009817753f),
		new Keyframe(0.8838775f, 0.001939224f),
		new Keyframe(1f, 0f)
	});

	private static AnimationCurve shake = new AnimationCurve(new Keyframe[4]
	{
		new Keyframe(0f, 0f),
		new Keyframe(0.25f, 1f),
		new Keyframe(0.75f, -1f),
		new Keyframe(1f, 0f)
	});

	private static Transform trans;

	private static float timeTotal;

	private static TweenAction tweenAction;

	private static float ratioPassed;

	private static float from;

	private static float val;

	private static bool isTweenFinished;

	private static int maxTweenReached;

	private static Vector3 newVect;

	private static GameObject target;

	private static GameObject customTarget;

	public static int startSearch = 0;

	public static LTDescr d;

	private static Action<LTEvent>[] eventListeners;

	private static GameObject[] goListeners;

	private static int eventsMaxSearch = 0;

	public static int EVENTS_MAX = 10;

	public static int LISTENERS_MAX = 10;

	private static int INIT_LISTENERS_MAX = LISTENERS_MAX;

	public static int maxSearch => tweenMaxSearch;

	public static int maxSimulataneousTweens => maxTweens;

	public static int tweensRunning
	{
		get
		{
			int num = 0;
			for (int i = 0; i <= tweenMaxSearch; i++)
			{
				if (tweens[i].toggle)
				{
					num++;
				}
			}
			return num;
		}
	}

	public static GameObject tweenEmpty
	{
		get
		{
			init(maxTweens);
			return _tweenEmpty;
		}
	}

	public static void init()
	{
		init(maxTweens);
	}

	public static void init(int maxSimultaneousTweens)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected Obj, but got Unknown
		if (tweens == null)
		{
			maxTweens = maxSimultaneousTweens;
			tweens = new LTDescrImpl[maxTweens];
			tweensFinished = new int[maxTweens];
			_tweenEmpty = new GameObject();
			((Object)_tweenEmpty).name = "~LeanTween";
			_tweenEmpty.transform.SetAsFirstSibling();
			_tweenEmpty.AddComponent(typeof(LeanTween));
			_tweenEmpty.isStatic = true;
			((Object)_tweenEmpty).hideFlags = (HideFlags)61;
			Object.DontDestroyOnLoad((Object)(object)_tweenEmpty);
			for (int i = 0; i < maxTweens; i++)
			{
				tweens[i] = new LTDescrImpl();
			}
			SceneManager.sceneLoaded += onLevelWasLoaded54;
		}
	}

	public static void reset()
	{
		for (int i = 0; i <= tweenMaxSearch; i++)
		{
			tweens[i].toggle = false;
		}
		tweens = null;
		Object.Destroy((Object)(object)_tweenEmpty);
	}

	public void Update()
	{
		update();
	}

	private static void onLevelWasLoaded54(Scene scene, LoadSceneMode mode)
	{
		internalOnLevelWasLoaded(scene.buildIndex);
	}

	private static void internalOnLevelWasLoaded(int lvl)
	{
		LTGUI.reset();
	}

	public static void update()
	{
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_3057: Unknown result type (might be due to invalid IL or missing references)
		//IL_3061: Unknown result type (might be due to invalid IL or missing references)
		//IL_306b: Unknown result type (might be due to invalid IL or missing references)
		//IL_307a: Expected Obj, but got Unknown
		//IL_30b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_30c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_30cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_30dc: Expected Obj, but got Unknown
		//IL_1856: Unknown result type (might be due to invalid IL or missing references)
		//IL_185b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1877: Unknown result type (might be due to invalid IL or missing references)
		//IL_1886: Unknown result type (might be due to invalid IL or missing references)
		//IL_189c: Unknown result type (might be due to invalid IL or missing references)
		//IL_18ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_18c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_18d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_18e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_18e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03da: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0409: Unknown result type (might be due to invalid IL or missing references)
		//IL_042c: Unknown result type (might be due to invalid IL or missing references)
		//IL_043b: Unknown result type (might be due to invalid IL or missing references)
		//IL_045e: Unknown result type (might be due to invalid IL or missing references)
		//IL_046d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0490: Unknown result type (might be due to invalid IL or missing references)
		//IL_049f: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0503: Unknown result type (might be due to invalid IL or missing references)
		//IL_0526: Unknown result type (might be due to invalid IL or missing references)
		//IL_0535: Unknown result type (might be due to invalid IL or missing references)
		//IL_0558: Unknown result type (might be due to invalid IL or missing references)
		//IL_0567: Unknown result type (might be due to invalid IL or missing references)
		//IL_058a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0599: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_05fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0620: Unknown result type (might be due to invalid IL or missing references)
		//IL_062f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0652: Unknown result type (might be due to invalid IL or missing references)
		//IL_0661: Unknown result type (might be due to invalid IL or missing references)
		//IL_0684: Unknown result type (might be due to invalid IL or missing references)
		//IL_0693: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_071a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0729: Unknown result type (might be due to invalid IL or missing references)
		//IL_074c: Unknown result type (might be due to invalid IL or missing references)
		//IL_075b: Unknown result type (might be due to invalid IL or missing references)
		//IL_077e: Unknown result type (might be due to invalid IL or missing references)
		//IL_078d: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_07bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0814: Unknown result type (might be due to invalid IL or missing references)
		//IL_0823: Unknown result type (might be due to invalid IL or missing references)
		//IL_0846: Unknown result type (might be due to invalid IL or missing references)
		//IL_0855: Unknown result type (might be due to invalid IL or missing references)
		//IL_0878: Unknown result type (might be due to invalid IL or missing references)
		//IL_0887: Unknown result type (might be due to invalid IL or missing references)
		//IL_08aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_08e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_08f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0922: Unknown result type (might be due to invalid IL or missing references)
		//IL_0931: Unknown result type (might be due to invalid IL or missing references)
		//IL_095e: Unknown result type (might be due to invalid IL or missing references)
		//IL_096d: Unknown result type (might be due to invalid IL or missing references)
		//IL_09a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_09b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_09ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_09f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ade: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aed: Unknown result type (might be due to invalid IL or missing references)
		//IL_2c2a: Unknown result type (might be due to invalid IL or missing references)
		//IL_2c39: Unknown result type (might be due to invalid IL or missing references)
		//IL_2c4f: Unknown result type (might be due to invalid IL or missing references)
		//IL_2c5e: Unknown result type (might be due to invalid IL or missing references)
		//IL_2c74: Unknown result type (might be due to invalid IL or missing references)
		//IL_2c83: Unknown result type (might be due to invalid IL or missing references)
		//IL_2c94: Unknown result type (might be due to invalid IL or missing references)
		//IL_2c99: Unknown result type (might be due to invalid IL or missing references)
		//IL_2cac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b0d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b1c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b48: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b57: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b61: Unknown result type (might be due to invalid IL or missing references)
		//IL_19a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_19b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_19c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_19d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_19f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a00: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a14: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a19: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a28: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a37: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a50: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a5f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a78: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a87: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a9b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1aa0: Unknown result type (might be due to invalid IL or missing references)
		//IL_1aaf: Unknown result type (might be due to invalid IL or missing references)
		//IL_1abe: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ad7: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ae6: Unknown result type (might be due to invalid IL or missing references)
		//IL_1aff: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b0e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b22: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b27: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b36: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b45: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b5e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b6d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b86: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b95: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ba9: Unknown result type (might be due to invalid IL or missing references)
		//IL_1bae: Unknown result type (might be due to invalid IL or missing references)
		//IL_1bbd: Unknown result type (might be due to invalid IL or missing references)
		//IL_1bcc: Unknown result type (might be due to invalid IL or missing references)
		//IL_1be5: Unknown result type (might be due to invalid IL or missing references)
		//IL_1bf4: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c0d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c1c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c30: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c35: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c44: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c53: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c6c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c7b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c94: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ca3: Unknown result type (might be due to invalid IL or missing references)
		//IL_1cb7: Unknown result type (might be due to invalid IL or missing references)
		//IL_1cbc: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ccb: Unknown result type (might be due to invalid IL or missing references)
		//IL_1cda: Unknown result type (might be due to invalid IL or missing references)
		//IL_1cf3: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d02: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d1b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d2a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d3e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d43: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d52: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d61: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d7a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d89: Unknown result type (might be due to invalid IL or missing references)
		//IL_1da2: Unknown result type (might be due to invalid IL or missing references)
		//IL_1db1: Unknown result type (might be due to invalid IL or missing references)
		//IL_1dc5: Unknown result type (might be due to invalid IL or missing references)
		//IL_1dca: Unknown result type (might be due to invalid IL or missing references)
		//IL_1dd9: Unknown result type (might be due to invalid IL or missing references)
		//IL_1de8: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e01: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e10: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e29: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e38: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e4c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e51: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e60: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e6f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e88: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e97: Unknown result type (might be due to invalid IL or missing references)
		//IL_1eb0: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ebf: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ed3: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ed8: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ee7: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ef6: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f0f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f1e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f37: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f46: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f5a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f5f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f6e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f7d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f96: Unknown result type (might be due to invalid IL or missing references)
		//IL_1fa5: Unknown result type (might be due to invalid IL or missing references)
		//IL_1fbe: Unknown result type (might be due to invalid IL or missing references)
		//IL_1fcd: Unknown result type (might be due to invalid IL or missing references)
		//IL_1fe1: Unknown result type (might be due to invalid IL or missing references)
		//IL_1fe6: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ff5: Unknown result type (might be due to invalid IL or missing references)
		//IL_2004: Unknown result type (might be due to invalid IL or missing references)
		//IL_201d: Unknown result type (might be due to invalid IL or missing references)
		//IL_202c: Unknown result type (might be due to invalid IL or missing references)
		//IL_2045: Unknown result type (might be due to invalid IL or missing references)
		//IL_2054: Unknown result type (might be due to invalid IL or missing references)
		//IL_2068: Unknown result type (might be due to invalid IL or missing references)
		//IL_206d: Unknown result type (might be due to invalid IL or missing references)
		//IL_207c: Unknown result type (might be due to invalid IL or missing references)
		//IL_208b: Unknown result type (might be due to invalid IL or missing references)
		//IL_20a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_20b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_20cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_20db: Unknown result type (might be due to invalid IL or missing references)
		//IL_20ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_20f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_2103: Unknown result type (might be due to invalid IL or missing references)
		//IL_2112: Unknown result type (might be due to invalid IL or missing references)
		//IL_212b: Unknown result type (might be due to invalid IL or missing references)
		//IL_213a: Unknown result type (might be due to invalid IL or missing references)
		//IL_2153: Unknown result type (might be due to invalid IL or missing references)
		//IL_2162: Unknown result type (might be due to invalid IL or missing references)
		//IL_2176: Unknown result type (might be due to invalid IL or missing references)
		//IL_217b: Unknown result type (might be due to invalid IL or missing references)
		//IL_218a: Unknown result type (might be due to invalid IL or missing references)
		//IL_2199: Unknown result type (might be due to invalid IL or missing references)
		//IL_21b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_21c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_21da: Unknown result type (might be due to invalid IL or missing references)
		//IL_21e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_21fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_2202: Unknown result type (might be due to invalid IL or missing references)
		//IL_2211: Unknown result type (might be due to invalid IL or missing references)
		//IL_2220: Unknown result type (might be due to invalid IL or missing references)
		//IL_2239: Unknown result type (might be due to invalid IL or missing references)
		//IL_2248: Unknown result type (might be due to invalid IL or missing references)
		//IL_2261: Unknown result type (might be due to invalid IL or missing references)
		//IL_2270: Unknown result type (might be due to invalid IL or missing references)
		//IL_2284: Unknown result type (might be due to invalid IL or missing references)
		//IL_2289: Unknown result type (might be due to invalid IL or missing references)
		//IL_2298: Unknown result type (might be due to invalid IL or missing references)
		//IL_22a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_22c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_22cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_22e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_22f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_230b: Unknown result type (might be due to invalid IL or missing references)
		//IL_2310: Unknown result type (might be due to invalid IL or missing references)
		//IL_231f: Unknown result type (might be due to invalid IL or missing references)
		//IL_232e: Unknown result type (might be due to invalid IL or missing references)
		//IL_2347: Unknown result type (might be due to invalid IL or missing references)
		//IL_2356: Unknown result type (might be due to invalid IL or missing references)
		//IL_236f: Unknown result type (might be due to invalid IL or missing references)
		//IL_237e: Unknown result type (might be due to invalid IL or missing references)
		//IL_2392: Unknown result type (might be due to invalid IL or missing references)
		//IL_2397: Unknown result type (might be due to invalid IL or missing references)
		//IL_23a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_23b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_23ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_23dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_23f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_2405: Unknown result type (might be due to invalid IL or missing references)
		//IL_2419: Unknown result type (might be due to invalid IL or missing references)
		//IL_241e: Unknown result type (might be due to invalid IL or missing references)
		//IL_242d: Unknown result type (might be due to invalid IL or missing references)
		//IL_243c: Unknown result type (might be due to invalid IL or missing references)
		//IL_2455: Unknown result type (might be due to invalid IL or missing references)
		//IL_2464: Unknown result type (might be due to invalid IL or missing references)
		//IL_247d: Unknown result type (might be due to invalid IL or missing references)
		//IL_248c: Unknown result type (might be due to invalid IL or missing references)
		//IL_24a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_24a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_24b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_24c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_24dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_24eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_2504: Unknown result type (might be due to invalid IL or missing references)
		//IL_2513: Unknown result type (might be due to invalid IL or missing references)
		//IL_2527: Unknown result type (might be due to invalid IL or missing references)
		//IL_252c: Unknown result type (might be due to invalid IL or missing references)
		//IL_253b: Unknown result type (might be due to invalid IL or missing references)
		//IL_254a: Unknown result type (might be due to invalid IL or missing references)
		//IL_2563: Unknown result type (might be due to invalid IL or missing references)
		//IL_2572: Unknown result type (might be due to invalid IL or missing references)
		//IL_258b: Unknown result type (might be due to invalid IL or missing references)
		//IL_259a: Unknown result type (might be due to invalid IL or missing references)
		//IL_25ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_25b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_25c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_25d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_25ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_25f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_2612: Unknown result type (might be due to invalid IL or missing references)
		//IL_2621: Unknown result type (might be due to invalid IL or missing references)
		//IL_2635: Unknown result type (might be due to invalid IL or missing references)
		//IL_263a: Unknown result type (might be due to invalid IL or missing references)
		//IL_2649: Unknown result type (might be due to invalid IL or missing references)
		//IL_2658: Unknown result type (might be due to invalid IL or missing references)
		//IL_267b: Unknown result type (might be due to invalid IL or missing references)
		//IL_268a: Unknown result type (might be due to invalid IL or missing references)
		//IL_26ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_26bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_26da: Unknown result type (might be due to invalid IL or missing references)
		//IL_26df: Unknown result type (might be due to invalid IL or missing references)
		//IL_26ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_26fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_2720: Unknown result type (might be due to invalid IL or missing references)
		//IL_272f: Unknown result type (might be due to invalid IL or missing references)
		//IL_2752: Unknown result type (might be due to invalid IL or missing references)
		//IL_2761: Unknown result type (might be due to invalid IL or missing references)
		//IL_277f: Unknown result type (might be due to invalid IL or missing references)
		//IL_2784: Unknown result type (might be due to invalid IL or missing references)
		//IL_2793: Unknown result type (might be due to invalid IL or missing references)
		//IL_27a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_27c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_27d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_27f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_2806: Unknown result type (might be due to invalid IL or missing references)
		//IL_2824: Unknown result type (might be due to invalid IL or missing references)
		//IL_2829: Unknown result type (might be due to invalid IL or missing references)
		//IL_2838: Unknown result type (might be due to invalid IL or missing references)
		//IL_2847: Unknown result type (might be due to invalid IL or missing references)
		//IL_2874: Unknown result type (might be due to invalid IL or missing references)
		//IL_2883: Unknown result type (might be due to invalid IL or missing references)
		//IL_28b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_28bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_28e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_28ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_28fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_290a: Unknown result type (might be due to invalid IL or missing references)
		//IL_2937: Unknown result type (might be due to invalid IL or missing references)
		//IL_2946: Unknown result type (might be due to invalid IL or missing references)
		//IL_2973: Unknown result type (might be due to invalid IL or missing references)
		//IL_2982: Unknown result type (might be due to invalid IL or missing references)
		//IL_29aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_29af: Unknown result type (might be due to invalid IL or missing references)
		//IL_29be: Unknown result type (might be due to invalid IL or missing references)
		//IL_29cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_29fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_2a09: Unknown result type (might be due to invalid IL or missing references)
		//IL_2a36: Unknown result type (might be due to invalid IL or missing references)
		//IL_2a45: Unknown result type (might be due to invalid IL or missing references)
		//IL_2a6d: Unknown result type (might be due to invalid IL or missing references)
		//IL_2a72: Unknown result type (might be due to invalid IL or missing references)
		//IL_2ba6: Unknown result type (might be due to invalid IL or missing references)
		//IL_2bb5: Unknown result type (might be due to invalid IL or missing references)
		//IL_2bce: Unknown result type (might be due to invalid IL or missing references)
		//IL_2bdd: Unknown result type (might be due to invalid IL or missing references)
		//IL_2bf6: Unknown result type (might be due to invalid IL or missing references)
		//IL_2c05: Unknown result type (might be due to invalid IL or missing references)
		//IL_2c19: Unknown result type (might be due to invalid IL or missing references)
		//IL_2c1e: Unknown result type (might be due to invalid IL or missing references)
		//IL_2cc9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a76: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a85: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aa4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ab3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b7f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b93: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b9d: Unknown result type (might be due to invalid IL or missing references)
		//IL_2ce6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bbb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bd9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bfa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c09: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c13: Unknown result type (might be due to invalid IL or missing references)
		//IL_2f7f: Unknown result type (might be due to invalid IL or missing references)
		//IL_2ac2: Unknown result type (might be due to invalid IL or missing references)
		//IL_2acc: Unknown result type (might be due to invalid IL or missing references)
		//IL_2ad1: Unknown result type (might be due to invalid IL or missing references)
		//IL_2ad6: Unknown result type (might be due to invalid IL or missing references)
		//IL_2ae5: Unknown result type (might be due to invalid IL or missing references)
		//IL_2aef: Unknown result type (might be due to invalid IL or missing references)
		//IL_2af4: Unknown result type (might be due to invalid IL or missing references)
		//IL_2d03: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c34: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c48: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c52: Unknown result type (might be due to invalid IL or missing references)
		//IL_2fab: Unknown result type (might be due to invalid IL or missing references)
		//IL_2b1a: Unknown result type (might be due to invalid IL or missing references)
		//IL_2b29: Unknown result type (might be due to invalid IL or missing references)
		//IL_2b3d: Unknown result type (might be due to invalid IL or missing references)
		//IL_2b4c: Unknown result type (might be due to invalid IL or missing references)
		//IL_2b60: Unknown result type (might be due to invalid IL or missing references)
		//IL_2b6f: Unknown result type (might be due to invalid IL or missing references)
		//IL_2b7e: Unknown result type (might be due to invalid IL or missing references)
		//IL_2d20: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c73: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c82: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c91: Unknown result type (might be due to invalid IL or missing references)
		//IL_2fe1: Unknown result type (might be due to invalid IL or missing references)
		//IL_2b92: Unknown result type (might be due to invalid IL or missing references)
		//IL_2b97: Unknown result type (might be due to invalid IL or missing references)
		//IL_2d3d: Unknown result type (might be due to invalid IL or missing references)
		//IL_2d7d: Unknown result type (might be due to invalid IL or missing references)
		//IL_2d82: Unknown result type (might be due to invalid IL or missing references)
		//IL_2d95: Unknown result type (might be due to invalid IL or missing references)
		//IL_2d9a: Unknown result type (might be due to invalid IL or missing references)
		//IL_2da3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d1d: Unknown result type (might be due to invalid IL or missing references)
		//IL_2dd9: Unknown result type (might be due to invalid IL or missing references)
		//IL_2dde: Unknown result type (might be due to invalid IL or missing references)
		//IL_0da9: Unknown result type (might be due to invalid IL or missing references)
		//IL_2e05: Unknown result type (might be due to invalid IL or missing references)
		//IL_2e0a: Unknown result type (might be due to invalid IL or missing references)
		//IL_2e1d: Unknown result type (might be due to invalid IL or missing references)
		//IL_2e22: Unknown result type (might be due to invalid IL or missing references)
		//IL_2e3f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e35: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ee9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ef8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f02: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ec2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f24: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f38: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f42: Unknown result type (might be due to invalid IL or missing references)
		//IL_2ea9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f64: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f73: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f82: Unknown result type (might be due to invalid IL or missing references)
		//IL_2ec8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fa9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fb8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fc2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fe4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ff8: Unknown result type (might be due to invalid IL or missing references)
		//IL_1002: Unknown result type (might be due to invalid IL or missing references)
		//IL_1024: Unknown result type (might be due to invalid IL or missing references)
		//IL_1033: Unknown result type (might be due to invalid IL or missing references)
		//IL_1042: Unknown result type (might be due to invalid IL or missing references)
		//IL_1062: Unknown result type (might be due to invalid IL or missing references)
		//IL_1067: Unknown result type (might be due to invalid IL or missing references)
		//IL_1072: Unknown result type (might be due to invalid IL or missing references)
		//IL_1077: Unknown result type (might be due to invalid IL or missing references)
		//IL_107c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1082: Unknown result type (might be due to invalid IL or missing references)
		//IL_1088: Unknown result type (might be due to invalid IL or missing references)
		//IL_1098: Unknown result type (might be due to invalid IL or missing references)
		//IL_109e: Unknown result type (might be due to invalid IL or missing references)
		//IL_10a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_10a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_10af: Unknown result type (might be due to invalid IL or missing references)
		//IL_10b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_10b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_10c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_10da: Unknown result type (might be due to invalid IL or missing references)
		//IL_10df: Unknown result type (might be due to invalid IL or missing references)
		//IL_10e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_10ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_10f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_1115: Unknown result type (might be due to invalid IL or missing references)
		//IL_111a: Unknown result type (might be due to invalid IL or missing references)
		//IL_112b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1130: Unknown result type (might be due to invalid IL or missing references)
		//IL_113f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1144: Unknown result type (might be due to invalid IL or missing references)
		//IL_1154: Unknown result type (might be due to invalid IL or missing references)
		//IL_115b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1160: Unknown result type (might be due to invalid IL or missing references)
		//IL_1165: Unknown result type (might be due to invalid IL or missing references)
		//IL_116c: Unknown result type (might be due to invalid IL or missing references)
		//IL_116e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1170: Unknown result type (might be due to invalid IL or missing references)
		//IL_1184: Unknown result type (might be due to invalid IL or missing references)
		//IL_1198: Unknown result type (might be due to invalid IL or missing references)
		//IL_119d: Unknown result type (might be due to invalid IL or missing references)
		//IL_11a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_11a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_11b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_11ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_12b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_12bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_12bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_12c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_12cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_12da: Unknown result type (might be due to invalid IL or missing references)
		//IL_12df: Unknown result type (might be due to invalid IL or missing references)
		//IL_12e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_1200: Unknown result type (might be due to invalid IL or missing references)
		//IL_1214: Unknown result type (might be due to invalid IL or missing references)
		//IL_1228: Unknown result type (might be due to invalid IL or missing references)
		//IL_1237: Unknown result type (might be due to invalid IL or missing references)
		//IL_1331: Unknown result type (might be due to invalid IL or missing references)
		//IL_1336: Unknown result type (might be due to invalid IL or missing references)
		//IL_1354: Unknown result type (might be due to invalid IL or missing references)
		//IL_1360: Unknown result type (might be due to invalid IL or missing references)
		//IL_12ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_12f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_1377: Unknown result type (might be due to invalid IL or missing references)
		//IL_148e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1493: Unknown result type (might be due to invalid IL or missing references)
		//IL_149f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1419: Unknown result type (might be due to invalid IL or missing references)
		//IL_141e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1436: Unknown result type (might be due to invalid IL or missing references)
		//IL_13aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_13de: Unknown result type (might be due to invalid IL or missing references)
		//IL_14e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_14c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_1558: Unknown result type (might be due to invalid IL or missing references)
		//IL_155d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1569: Unknown result type (might be due to invalid IL or missing references)
		//IL_15e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_15ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_15f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_15fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_1607: Unknown result type (might be due to invalid IL or missing references)
		//IL_1617: Unknown result type (might be due to invalid IL or missing references)
		//IL_161b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1620: Unknown result type (might be due to invalid IL or missing references)
		//IL_1625: Unknown result type (might be due to invalid IL or missing references)
		//IL_1629: Unknown result type (might be due to invalid IL or missing references)
		//IL_162b: Unknown result type (might be due to invalid IL or missing references)
		//IL_162d: Unknown result type (might be due to invalid IL or missing references)
		//IL_163e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1651: Unknown result type (might be due to invalid IL or missing references)
		//IL_1656: Unknown result type (might be due to invalid IL or missing references)
		//IL_1660: Unknown result type (might be due to invalid IL or missing references)
		//IL_168e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1693: Unknown result type (might be due to invalid IL or missing references)
		//IL_169e: Unknown result type (might be due to invalid IL or missing references)
		//IL_16a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_16af: Unknown result type (might be due to invalid IL or missing references)
		//IL_16b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_16c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_16c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_16cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_16d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_16d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_16d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_16da: Unknown result type (might be due to invalid IL or missing references)
		//IL_16eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_16fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_1703: Unknown result type (might be due to invalid IL or missing references)
		//IL_170f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1714: Unknown result type (might be due to invalid IL or missing references)
		//IL_1592: Unknown result type (might be due to invalid IL or missing references)
		//IL_15c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_1772: Unknown result type (might be due to invalid IL or missing references)
		//IL_1777: Unknown result type (might be due to invalid IL or missing references)
		//IL_1788: Unknown result type (might be due to invalid IL or missing references)
		//IL_178f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1796: Unknown result type (might be due to invalid IL or missing references)
		//IL_17b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_17bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_17c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_17d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_17dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_1801: Unknown result type (might be due to invalid IL or missing references)
		//IL_1806: Unknown result type (might be due to invalid IL or missing references)
		//IL_1812: Unknown result type (might be due to invalid IL or missing references)
		//IL_1819: Unknown result type (might be due to invalid IL or missing references)
		//IL_1825: Unknown result type (might be due to invalid IL or missing references)
		if (frameRendered == Time.frameCount)
		{
			return;
		}
		init();
		if (dtEstimated < 0f)
		{
			dtEstimated = 0f;
		}
		else
		{
			dtEstimated = Time.unscaledDeltaTime;
		}
		dtActual = Time.deltaTime;
		maxTweenReached = 0;
		finishedCnt = 0;
		for (int i = 0; i <= tweenMaxSearch && i < maxTweens; i++)
		{
			if (!tweens[i].toggle)
			{
				continue;
			}
			maxTweenReached = i;
			tween = tweens[i];
			trans = tween.trans;
			timeTotal = tween.time;
			tweenAction = tween.type;
			if (tween.useEstimatedTime)
			{
				dt = dtEstimated;
			}
			else if (tween.useFrames)
			{
				dt = 1f;
			}
			else if (tween.useManualTime)
			{
				dt = dtManual;
			}
			else if (tween.direction == 0f)
			{
				dt = 0f;
			}
			else
			{
				dt = dtActual;
			}
			if ((Object)(object)trans == (Object)null)
			{
				removeTween(i);
				continue;
			}
			if (tweenAction == TweenAction.MOVE_TO_TRANSFORM)
			{
				tween.to = tween.toTrans.position;
				tween.diff = tween.to - tween.from;
			}
			isTweenFinished = false;
			if (tween.delay <= 0f)
			{
				if (tween.passed + dt > tween.time && tween.direction > 0f)
				{
					isTweenFinished = true;
					tween.passed = tween.time;
				}
				else if (tween.direction < 0f && tween.passed - dt < 0f)
				{
					isTweenFinished = true;
					tween.passed = Mathf.Epsilon;
				}
			}
			if (!tween.hasInitiliazed && (((double)tween.passed == 0.0 && (double)tween.delay == 0.0) || (double)tween.passed > 0.0))
			{
				tween.init();
			}
			if (tween.delay <= 0f && tween.direction != 0f)
			{
				if (timeTotal <= 0f)
				{
					ratioPassed = 1f;
				}
				else
				{
					ratioPassed = tween.passed / timeTotal;
				}
				if (ratioPassed > 1f)
				{
					ratioPassed = 1f;
				}
				else if (ratioPassed < 0f)
				{
					ratioPassed = 0f;
				}
				if (tweenAction >= TweenAction.MOVE_X && tweenAction < TweenAction.MOVE)
				{
					if (tween.animationCurve != null)
					{
						LeanTween.val = tweenOnCurve(tween, ratioPassed);
					}
					else
					{
						switch (tween.tweenType)
						{
						case LeanTweenType.linear:
							LeanTween.val = tween.from.x + tween.diff.x * ratioPassed;
							break;
						case LeanTweenType.easeOutQuad:
							LeanTween.val = easeOutQuadOpt(tween.from.x, tween.diff.x, ratioPassed);
							break;
						case LeanTweenType.easeInQuad:
							LeanTween.val = easeInQuadOpt(tween.from.x, tween.diff.x, ratioPassed);
							break;
						case LeanTweenType.easeInOutQuad:
							LeanTween.val = easeInOutQuadOpt(tween.from.x, tween.diff.x, ratioPassed);
							break;
						case LeanTweenType.easeInCubic:
							LeanTween.val = easeInCubic(tween.from.x, tween.to.x, ratioPassed);
							break;
						case LeanTweenType.easeOutCubic:
							LeanTween.val = easeOutCubic(tween.from.x, tween.to.x, ratioPassed);
							break;
						case LeanTweenType.easeInOutCubic:
							LeanTween.val = easeInOutCubic(tween.from.x, tween.to.x, ratioPassed);
							break;
						case LeanTweenType.easeInQuart:
							LeanTween.val = easeInQuart(tween.from.x, tween.to.x, ratioPassed);
							break;
						case LeanTweenType.easeOutQuart:
							LeanTween.val = easeOutQuart(tween.from.x, tween.to.x, ratioPassed);
							break;
						case LeanTweenType.easeInOutQuart:
							LeanTween.val = easeInOutQuart(tween.from.x, tween.to.x, ratioPassed);
							break;
						case LeanTweenType.easeInQuint:
							LeanTween.val = easeInQuint(tween.from.x, tween.to.x, ratioPassed);
							break;
						case LeanTweenType.easeOutQuint:
							LeanTween.val = easeOutQuint(tween.from.x, tween.to.x, ratioPassed);
							break;
						case LeanTweenType.easeInOutQuint:
							LeanTween.val = easeInOutQuint(tween.from.x, tween.to.x, ratioPassed);
							break;
						case LeanTweenType.easeInSine:
							LeanTween.val = easeInSine(tween.from.x, tween.to.x, ratioPassed);
							break;
						case LeanTweenType.easeOutSine:
							LeanTween.val = easeOutSine(tween.from.x, tween.to.x, ratioPassed);
							break;
						case LeanTweenType.easeInOutSine:
							LeanTween.val = easeInOutSine(tween.from.x, tween.to.x, ratioPassed);
							break;
						case LeanTweenType.easeInExpo:
							LeanTween.val = easeInExpo(tween.from.x, tween.to.x, ratioPassed);
							break;
						case LeanTweenType.easeOutExpo:
							LeanTween.val = easeOutExpo(tween.from.x, tween.to.x, ratioPassed);
							break;
						case LeanTweenType.easeInOutExpo:
							LeanTween.val = easeInOutExpo(tween.from.x, tween.to.x, ratioPassed);
							break;
						case LeanTweenType.easeInCirc:
							LeanTween.val = easeInCirc(tween.from.x, tween.to.x, ratioPassed);
							break;
						case LeanTweenType.easeOutCirc:
							LeanTween.val = easeOutCirc(tween.from.x, tween.to.x, ratioPassed);
							break;
						case LeanTweenType.easeInOutCirc:
							LeanTween.val = easeInOutCirc(tween.from.x, tween.to.x, ratioPassed);
							break;
						case LeanTweenType.easeInBounce:
							LeanTween.val = easeInBounce(tween.from.x, tween.to.x, ratioPassed);
							break;
						case LeanTweenType.easeOutBounce:
							LeanTween.val = easeOutBounce(tween.from.x, tween.to.x, ratioPassed);
							break;
						case LeanTweenType.easeInOutBounce:
							LeanTween.val = easeInOutBounce(tween.from.x, tween.to.x, ratioPassed);
							break;
						case LeanTweenType.easeInBack:
							LeanTween.val = easeInBack(tween.from.x, tween.to.x, ratioPassed, tween.overshoot);
							break;
						case LeanTweenType.easeOutBack:
							LeanTween.val = easeOutBack(tween.from.x, tween.to.x, ratioPassed, tween.overshoot);
							break;
						case LeanTweenType.easeInOutBack:
							LeanTween.val = easeInOutBack(tween.from.x, tween.to.x, ratioPassed, tween.overshoot);
							break;
						case LeanTweenType.easeInElastic:
							LeanTween.val = easeInElastic(tween.from.x, tween.to.x, ratioPassed, tween.overshoot, tween.period);
							break;
						case LeanTweenType.easeOutElastic:
							LeanTween.val = easeOutElastic(tween.from.x, tween.to.x, ratioPassed, tween.overshoot, tween.period);
							break;
						case LeanTweenType.easeInOutElastic:
							LeanTween.val = easeInOutElastic(tween.from.x, tween.to.x, ratioPassed, tween.overshoot, tween.period);
							break;
						case LeanTweenType.easeShake:
						case LeanTweenType.punch:
							if (tween.tweenType == LeanTweenType.punch)
							{
								tween.animationCurve = punch;
							}
							else if (tween.tweenType == LeanTweenType.easeShake)
							{
								tween.animationCurve = shake;
							}
							tween.toInternal.x = tween.from.x + tween.to.x;
							tween.diffInternal.x = tween.to.x - tween.from.x;
							LeanTween.val = tweenOnCurve(tween, ratioPassed);
							break;
						case LeanTweenType.easeSpring:
							LeanTween.val = spring(tween.from.x, tween.to.x, ratioPassed);
							break;
						default:
							LeanTween.val = tween.from.x + tween.diff.x * ratioPassed;
							break;
						}
					}
					if (tweenAction == TweenAction.MOVE_X)
					{
						trans.position = new Vector3(LeanTween.val, trans.position.y, trans.position.z);
					}
					else if (tweenAction == TweenAction.MOVE_Y)
					{
						trans.position = new Vector3(trans.position.x, LeanTween.val, trans.position.z);
					}
					else if (tweenAction == TweenAction.MOVE_Z)
					{
						trans.position = new Vector3(trans.position.x, trans.position.y, LeanTween.val);
					}
					if (tweenAction == TweenAction.MOVE_LOCAL_X)
					{
						trans.localPosition = new Vector3(LeanTween.val, trans.localPosition.y, trans.localPosition.z);
					}
					else if (tweenAction == TweenAction.MOVE_LOCAL_Y)
					{
						trans.localPosition = new Vector3(trans.localPosition.x, LeanTween.val, trans.localPosition.z);
					}
					else if (tweenAction == TweenAction.MOVE_LOCAL_Z)
					{
						trans.localPosition = new Vector3(trans.localPosition.x, trans.localPosition.y, LeanTween.val);
					}
					else if (tweenAction == TweenAction.MOVE_CURVED)
					{
						if (tween.path.orientToPath)
						{
							if (tween.path.orientToPath2d)
							{
								tween.path.place2d(trans, LeanTween.val);
							}
							else
							{
								tween.path.place(trans, LeanTween.val);
							}
						}
						else
						{
							trans.position = tween.path.point(LeanTween.val);
						}
					}
					else if (tweenAction == TweenAction.MOVE_CURVED_LOCAL)
					{
						if (tween.path.orientToPath)
						{
							if (tween.path.orientToPath2d)
							{
								tween.path.placeLocal2d(trans, LeanTween.val);
							}
							else
							{
								tween.path.placeLocal(trans, LeanTween.val);
							}
						}
						else
						{
							trans.localPosition = tween.path.point(LeanTween.val);
						}
					}
					else if (tweenAction == TweenAction.MOVE_SPLINE)
					{
						if (tween.spline.orientToPath)
						{
							if (tween.spline.orientToPath2d)
							{
								tween.spline.place2d(trans, LeanTween.val);
							}
							else
							{
								tween.spline.place(trans, LeanTween.val);
							}
						}
						else
						{
							trans.position = tween.spline.point(LeanTween.val);
						}
					}
					else if (tweenAction == TweenAction.MOVE_SPLINE_LOCAL)
					{
						if (tween.spline.orientToPath)
						{
							if (tween.spline.orientToPath2d)
							{
								tween.spline.placeLocal2d(trans, LeanTween.val);
							}
							else
							{
								tween.spline.placeLocal(trans, LeanTween.val);
							}
						}
						else
						{
							trans.localPosition = tween.spline.point(LeanTween.val);
						}
					}
					else if (tweenAction == TweenAction.SCALE_X)
					{
						trans.localScale = new Vector3(LeanTween.val, trans.localScale.y, trans.localScale.z);
					}
					else if (tweenAction == TweenAction.SCALE_Y)
					{
						trans.localScale = new Vector3(trans.localScale.x, LeanTween.val, trans.localScale.z);
					}
					else if (tweenAction == TweenAction.SCALE_Z)
					{
						trans.localScale = new Vector3(trans.localScale.x, trans.localScale.y, LeanTween.val);
					}
					else if (tweenAction == TweenAction.ROTATE_X)
					{
						trans.eulerAngles = new Vector3(LeanTween.val, trans.eulerAngles.y, trans.eulerAngles.z);
					}
					else if (tweenAction == TweenAction.ROTATE_Y)
					{
						trans.eulerAngles = new Vector3(trans.eulerAngles.x, LeanTween.val, trans.eulerAngles.z);
					}
					else if (tweenAction == TweenAction.ROTATE_Z)
					{
						trans.eulerAngles = new Vector3(trans.eulerAngles.x, trans.eulerAngles.y, LeanTween.val);
					}
					else if (tweenAction == TweenAction.ROTATE_AROUND)
					{
						Vector3 localPosition = trans.localPosition;
						Vector3 val = trans.TransformPoint(tween.point);
						trans.RotateAround(val, tween.axis, 0f - LeanTween.val);
						Vector3 val2 = localPosition - trans.localPosition;
						trans.localPosition = localPosition - val2;
						trans.rotation = tween.origRotation;
						val = trans.TransformPoint(tween.point);
						trans.RotateAround(val, tween.axis, LeanTween.val);
					}
					else if (tweenAction == TweenAction.ROTATE_AROUND_LOCAL)
					{
						Vector3 localPosition2 = trans.localPosition;
						trans.RotateAround(trans.TransformPoint(tween.point), trans.TransformDirection(tween.axis), 0f - LeanTween.val);
						Vector3 val3 = localPosition2 - trans.localPosition;
						trans.localPosition = localPosition2 - val3;
						trans.localRotation = tween.origRotation;
						Vector3 val4 = trans.TransformPoint(tween.point);
						trans.RotateAround(val4, trans.TransformDirection(tween.axis), LeanTween.val);
					}
					else if (tweenAction == TweenAction.ALPHA)
					{
						if ((Object)(object)tween.spriteRen != (Object)null)
						{
							tween.spriteRen.color = new Color(tween.spriteRen.color.r, tween.spriteRen.color.g, tween.spriteRen.color.b, LeanTween.val);
							alphaRecursiveSprite(tween.trans, LeanTween.val);
						}
						else
						{
							alphaRecursive(tween.trans, LeanTween.val, tween.useRecursion);
						}
					}
					else if (tweenAction == TweenAction.ALPHA_VERTEX)
					{
						Mesh mesh = ((Component)trans).GetComponent<MeshFilter>().mesh;
						Vector3[] vertices = mesh.vertices;
						Color32[] array = new Color32[vertices.Length];
						Color32 val5 = mesh.colors32[0];
						val5 = Color32.op_Implicit(new Color((float)(int)val5.r, (float)(int)val5.g, (float)(int)val5.b, LeanTween.val));
						for (int j = 0; j < vertices.Length; j++)
						{
							array[j] = val5;
						}
						mesh.colors32 = array;
					}
					else if (tweenAction == TweenAction.COLOR || tweenAction == TweenAction.CALLBACK_COLOR)
					{
						Color val6 = tweenColor(tween, LeanTween.val);
						if ((Object)(object)tween.spriteRen != (Object)null)
						{
							tween.spriteRen.color = val6;
							colorRecursiveSprite(trans, val6);
						}
						else if (tweenAction == TweenAction.COLOR)
						{
							colorRecursive(trans, val6, tween.useRecursion);
						}
						if (dt != 0f && tween.onUpdateColor != null)
						{
							tween.onUpdateColor(val6);
						}
						else if (dt != 0f && tween.onUpdateColorObject != null)
						{
							tween.onUpdateColorObject(val6, tween.onUpdateParam);
						}
					}
					else if (tweenAction == TweenAction.CANVAS_ALPHA)
					{
						if ((Object)(object)tween.uiImage != (Object)null)
						{
							Color val7 = ((Graphic)tween.uiImage).color;
							val7.a = LeanTween.val;
							((Graphic)tween.uiImage).color = val7;
						}
						if (tween.useRecursion)
						{
							alphaRecursive(tween.rectTransform, LeanTween.val);
							textAlphaRecursive((Transform)(object)tween.rectTransform, LeanTween.val);
						}
					}
					else if (tweenAction == TweenAction.CANVAS_COLOR)
					{
						Color val8 = tweenColor(tween, LeanTween.val);
						((Graphic)tween.uiImage).color = val8;
						if (dt != 0f && tween.onUpdateColor != null)
						{
							tween.onUpdateColor(val8);
						}
						if (tween.useRecursion)
						{
							colorRecursive(tween.rectTransform, val8);
						}
					}
					else if (tweenAction == TweenAction.CANVASGROUP_ALPHA)
					{
						((Component)tween.trans).GetComponent<CanvasGroup>().alpha = LeanTween.val;
					}
					else if (tweenAction == TweenAction.TEXT_ALPHA)
					{
						textAlphaRecursive(trans, LeanTween.val, tween.useRecursion);
					}
					else if (tweenAction == TweenAction.TEXT_COLOR)
					{
						Color val9 = tweenColor(tween, LeanTween.val);
						((Graphic)tween.uiText).color = val9;
						if (dt != 0f && tween.onUpdateColor != null)
						{
							tween.onUpdateColor(val9);
						}
						if (tween.useRecursion && trans.childCount > 0)
						{
							textColorRecursive(tween.trans, val9);
						}
					}
					else if (tweenAction == TweenAction.CANVAS_ROTATEAROUND)
					{
						RectTransform rectTransform = tween.rectTransform;
						Vector3 localPosition3 = ((Transform)rectTransform).localPosition;
						((Transform)rectTransform).RotateAround(((Transform)rectTransform).TransformPoint(tween.point), tween.axis, 0f - LeanTween.val);
						Vector3 val10 = localPosition3 - ((Transform)rectTransform).localPosition;
						((Transform)rectTransform).localPosition = localPosition3 - val10;
						((Transform)rectTransform).rotation = tween.origRotation;
						((Transform)rectTransform).RotateAround(((Transform)rectTransform).TransformPoint(tween.point), tween.axis, LeanTween.val);
					}
					else if (tweenAction == TweenAction.CANVAS_ROTATEAROUND_LOCAL)
					{
						RectTransform rectTransform2 = tween.rectTransform;
						Vector3 localPosition4 = ((Transform)rectTransform2).localPosition;
						((Transform)rectTransform2).RotateAround(((Transform)rectTransform2).TransformPoint(tween.point), ((Transform)rectTransform2).TransformDirection(tween.axis), 0f - LeanTween.val);
						Vector3 val11 = localPosition4 - ((Transform)rectTransform2).localPosition;
						((Transform)rectTransform2).localPosition = localPosition4 - val11;
						((Transform)rectTransform2).rotation = tween.origRotation;
						((Transform)rectTransform2).RotateAround(((Transform)rectTransform2).TransformPoint(tween.point), ((Transform)rectTransform2).TransformDirection(tween.axis), LeanTween.val);
					}
					else if (tweenAction == TweenAction.CANVAS_PLAYSPRITE)
					{
						int num = (int)Mathf.Round(LeanTween.val);
						tween.uiImage.sprite = tween.sprites[num];
					}
					else if (tweenAction == TweenAction.CANVAS_MOVE_X)
					{
						Vector3 anchoredPosition3D = tween.rectTransform.anchoredPosition3D;
						tween.rectTransform.anchoredPosition3D = new Vector3(LeanTween.val, anchoredPosition3D.y, anchoredPosition3D.z);
					}
					else if (tweenAction == TweenAction.CANVAS_MOVE_Y)
					{
						Vector3 anchoredPosition3D2 = tween.rectTransform.anchoredPosition3D;
						tween.rectTransform.anchoredPosition3D = new Vector3(anchoredPosition3D2.x, LeanTween.val, anchoredPosition3D2.z);
					}
					else if (tweenAction == TweenAction.CANVAS_MOVE_Z)
					{
						Vector3 anchoredPosition3D3 = tween.rectTransform.anchoredPosition3D;
						tween.rectTransform.anchoredPosition3D = new Vector3(anchoredPosition3D3.x, anchoredPosition3D3.y, LeanTween.val);
					}
				}
				else if (tweenAction >= TweenAction.MOVE)
				{
					if (tween.animationCurve != null)
					{
						newVect = tweenOnCurveVector(tween, ratioPassed);
					}
					else if (tween.tweenType == LeanTweenType.linear)
					{
						newVect = new Vector3(tween.from.x + tween.diff.x * ratioPassed, tween.from.y + tween.diff.y * ratioPassed, tween.from.z + tween.diff.z * ratioPassed);
					}
					else if (tween.tweenType >= LeanTweenType.linear)
					{
						switch (tween.tweenType)
						{
						case LeanTweenType.easeOutQuad:
							newVect = new Vector3(easeOutQuadOpt(tween.from.x, tween.diff.x, ratioPassed), easeOutQuadOpt(tween.from.y, tween.diff.y, ratioPassed), easeOutQuadOpt(tween.from.z, tween.diff.z, ratioPassed));
							break;
						case LeanTweenType.easeInQuad:
							newVect = new Vector3(easeInQuadOpt(tween.from.x, tween.diff.x, ratioPassed), easeInQuadOpt(tween.from.y, tween.diff.y, ratioPassed), easeInQuadOpt(tween.from.z, tween.diff.z, ratioPassed));
							break;
						case LeanTweenType.easeInOutQuad:
							newVect = new Vector3(easeInOutQuadOpt(tween.from.x, tween.diff.x, ratioPassed), easeInOutQuadOpt(tween.from.y, tween.diff.y, ratioPassed), easeInOutQuadOpt(tween.from.z, tween.diff.z, ratioPassed));
							break;
						case LeanTweenType.easeInCubic:
							newVect = new Vector3(easeInCubic(tween.from.x, tween.to.x, ratioPassed), easeInCubic(tween.from.y, tween.to.y, ratioPassed), easeInCubic(tween.from.z, tween.to.z, ratioPassed));
							break;
						case LeanTweenType.easeOutCubic:
							newVect = new Vector3(easeOutCubic(tween.from.x, tween.to.x, ratioPassed), easeOutCubic(tween.from.y, tween.to.y, ratioPassed), easeOutCubic(tween.from.z, tween.to.z, ratioPassed));
							break;
						case LeanTweenType.easeInOutCubic:
							newVect = new Vector3(easeInOutCubic(tween.from.x, tween.to.x, ratioPassed), easeInOutCubic(tween.from.y, tween.to.y, ratioPassed), easeInOutCubic(tween.from.z, tween.to.z, ratioPassed));
							break;
						case LeanTweenType.easeInQuart:
							newVect = new Vector3(easeInQuart(tween.from.x, tween.to.x, ratioPassed), easeInQuart(tween.from.y, tween.to.y, ratioPassed), easeInQuart(tween.from.z, tween.to.z, ratioPassed));
							break;
						case LeanTweenType.easeOutQuart:
							newVect = new Vector3(easeOutQuart(tween.from.x, tween.to.x, ratioPassed), easeOutQuart(tween.from.y, tween.to.y, ratioPassed), easeOutQuart(tween.from.z, tween.to.z, ratioPassed));
							break;
						case LeanTweenType.easeInOutQuart:
							newVect = new Vector3(easeInOutQuart(tween.from.x, tween.to.x, ratioPassed), easeInOutQuart(tween.from.y, tween.to.y, ratioPassed), easeInOutQuart(tween.from.z, tween.to.z, ratioPassed));
							break;
						case LeanTweenType.easeInQuint:
							newVect = new Vector3(easeInQuint(tween.from.x, tween.to.x, ratioPassed), easeInQuint(tween.from.y, tween.to.y, ratioPassed), easeInQuint(tween.from.z, tween.to.z, ratioPassed));
							break;
						case LeanTweenType.easeOutQuint:
							newVect = new Vector3(easeOutQuint(tween.from.x, tween.to.x, ratioPassed), easeOutQuint(tween.from.y, tween.to.y, ratioPassed), easeOutQuint(tween.from.z, tween.to.z, ratioPassed));
							break;
						case LeanTweenType.easeInOutQuint:
							newVect = new Vector3(easeInOutQuint(tween.from.x, tween.to.x, ratioPassed), easeInOutQuint(tween.from.y, tween.to.y, ratioPassed), easeInOutQuint(tween.from.z, tween.to.z, ratioPassed));
							break;
						case LeanTweenType.easeInSine:
							newVect = new Vector3(easeInSine(tween.from.x, tween.to.x, ratioPassed), easeInSine(tween.from.y, tween.to.y, ratioPassed), easeInSine(tween.from.z, tween.to.z, ratioPassed));
							break;
						case LeanTweenType.easeOutSine:
							newVect = new Vector3(easeOutSine(tween.from.x, tween.to.x, ratioPassed), easeOutSine(tween.from.y, tween.to.y, ratioPassed), easeOutSine(tween.from.z, tween.to.z, ratioPassed));
							break;
						case LeanTweenType.easeInOutSine:
							newVect = new Vector3(easeInOutSine(tween.from.x, tween.to.x, ratioPassed), easeInOutSine(tween.from.y, tween.to.y, ratioPassed), easeInOutSine(tween.from.z, tween.to.z, ratioPassed));
							break;
						case LeanTweenType.easeInExpo:
							newVect = new Vector3(easeInExpo(tween.from.x, tween.to.x, ratioPassed), easeInExpo(tween.from.y, tween.to.y, ratioPassed), easeInExpo(tween.from.z, tween.to.z, ratioPassed));
							break;
						case LeanTweenType.easeOutExpo:
							newVect = new Vector3(easeOutExpo(tween.from.x, tween.to.x, ratioPassed), easeOutExpo(tween.from.y, tween.to.y, ratioPassed), easeOutExpo(tween.from.z, tween.to.z, ratioPassed));
							break;
						case LeanTweenType.easeInOutExpo:
							newVect = new Vector3(easeInOutExpo(tween.from.x, tween.to.x, ratioPassed), easeInOutExpo(tween.from.y, tween.to.y, ratioPassed), easeInOutExpo(tween.from.z, tween.to.z, ratioPassed));
							break;
						case LeanTweenType.easeInCirc:
							newVect = new Vector3(easeInCirc(tween.from.x, tween.to.x, ratioPassed), easeInCirc(tween.from.y, tween.to.y, ratioPassed), easeInCirc(tween.from.z, tween.to.z, ratioPassed));
							break;
						case LeanTweenType.easeOutCirc:
							newVect = new Vector3(easeOutCirc(tween.from.x, tween.to.x, ratioPassed), easeOutCirc(tween.from.y, tween.to.y, ratioPassed), easeOutCirc(tween.from.z, tween.to.z, ratioPassed));
							break;
						case LeanTweenType.easeInOutCirc:
							newVect = new Vector3(easeInOutCirc(tween.from.x, tween.to.x, ratioPassed), easeInOutCirc(tween.from.y, tween.to.y, ratioPassed), easeInOutCirc(tween.from.z, tween.to.z, ratioPassed));
							break;
						case LeanTweenType.easeInBounce:
							newVect = new Vector3(easeInBounce(tween.from.x, tween.to.x, ratioPassed), easeInBounce(tween.from.y, tween.to.y, ratioPassed), easeInBounce(tween.from.z, tween.to.z, ratioPassed));
							break;
						case LeanTweenType.easeOutBounce:
							newVect = new Vector3(easeOutBounce(tween.from.x, tween.to.x, ratioPassed), easeOutBounce(tween.from.y, tween.to.y, ratioPassed), easeOutBounce(tween.from.z, tween.to.z, ratioPassed));
							break;
						case LeanTweenType.easeInOutBounce:
							newVect = new Vector3(easeInOutBounce(tween.from.x, tween.to.x, ratioPassed), easeInOutBounce(tween.from.y, tween.to.y, ratioPassed), easeInOutBounce(tween.from.z, tween.to.z, ratioPassed));
							break;
						case LeanTweenType.easeInBack:
							newVect = new Vector3(easeInBack(tween.from.x, tween.to.x, ratioPassed, tween.overshoot), easeInBack(tween.from.y, tween.to.y, ratioPassed, tween.overshoot), easeInBack(tween.from.z, tween.to.z, ratioPassed, tween.overshoot));
							break;
						case LeanTweenType.easeOutBack:
							newVect = new Vector3(easeOutBack(tween.from.x, tween.to.x, ratioPassed, tween.overshoot), easeOutBack(tween.from.y, tween.to.y, ratioPassed, tween.overshoot), easeOutBack(tween.from.z, tween.to.z, ratioPassed, tween.overshoot));
							break;
						case LeanTweenType.easeInOutBack:
							newVect = new Vector3(easeInOutBack(tween.from.x, tween.to.x, ratioPassed, tween.overshoot), easeInOutBack(tween.from.y, tween.to.y, ratioPassed, tween.overshoot), easeInOutBack(tween.from.z, tween.to.z, ratioPassed, tween.overshoot));
							break;
						case LeanTweenType.easeInElastic:
							newVect = new Vector3(easeInElastic(tween.from.x, tween.to.x, ratioPassed, tween.overshoot, tween.period), easeInElastic(tween.from.y, tween.to.y, ratioPassed, tween.overshoot, tween.period), easeInElastic(tween.from.z, tween.to.z, ratioPassed, tween.overshoot, tween.period));
							break;
						case LeanTweenType.easeOutElastic:
							newVect = new Vector3(easeOutElastic(tween.from.x, tween.to.x, ratioPassed, tween.overshoot, tween.period), easeOutElastic(tween.from.y, tween.to.y, ratioPassed, tween.overshoot, tween.period), easeOutElastic(tween.from.z, tween.to.z, ratioPassed, tween.overshoot, tween.period));
							break;
						case LeanTweenType.easeInOutElastic:
							newVect = new Vector3(easeInOutElastic(tween.from.x, tween.to.x, ratioPassed, tween.overshoot, tween.period), easeInOutElastic(tween.from.y, tween.to.y, ratioPassed, tween.overshoot, tween.period), easeInOutElastic(tween.from.z, tween.to.z, ratioPassed, tween.overshoot, tween.period));
							break;
						case LeanTweenType.easeShake:
						case LeanTweenType.punch:
							if (tween.tweenType == LeanTweenType.punch)
							{
								tween.animationCurve = punch;
							}
							else if (tween.tweenType == LeanTweenType.easeShake)
							{
								tween.animationCurve = shake;
							}
							tween.toInternal = tween.from + tween.to;
							tween.diff = tween.to - tween.from;
							if (tweenAction == TweenAction.ROTATE || tweenAction == TweenAction.ROTATE_LOCAL)
							{
								tween.to = new Vector3(closestRot(tween.from.x, tween.to.x), closestRot(tween.from.y, tween.to.y), closestRot(tween.from.z, tween.to.z));
							}
							newVect = tweenOnCurveVector(tween, ratioPassed);
							break;
						case LeanTweenType.easeSpring:
							newVect = new Vector3(spring(tween.from.x, tween.to.x, ratioPassed), spring(tween.from.y, tween.to.y, ratioPassed), spring(tween.from.z, tween.to.z, ratioPassed));
							break;
						}
					}
					else
					{
						newVect = new Vector3(tween.from.x + tween.diff.x * ratioPassed, tween.from.y + tween.diff.y * ratioPassed, tween.from.z + tween.diff.z * ratioPassed);
					}
					Rect rect;
					if (tweenAction == TweenAction.MOVE)
					{
						trans.position = newVect;
					}
					else if (tweenAction == TweenAction.MOVE_LOCAL)
					{
						trans.localPosition = newVect;
					}
					else if (tweenAction == TweenAction.MOVE_TO_TRANSFORM)
					{
						trans.position = newVect;
					}
					else if (tweenAction == TweenAction.ROTATE)
					{
						trans.eulerAngles = newVect;
					}
					else if (tweenAction == TweenAction.ROTATE_LOCAL)
					{
						trans.localEulerAngles = newVect;
					}
					else if (tweenAction == TweenAction.SCALE)
					{
						trans.localScale = newVect;
					}
					else if (tweenAction == TweenAction.GUI_MOVE)
					{
						LTRect ltRect = tween.ltRect;
						float x = newVect.x;
						float y = newVect.y;
						rect = tween.ltRect.rect;
						float width = rect.width;
						rect = tween.ltRect.rect;
						ltRect.rect = new Rect(x, y, width, rect.height);
					}
					else if (tweenAction == TweenAction.GUI_MOVE_MARGIN)
					{
						tween.ltRect.margin = new Vector2(newVect.x, newVect.y);
					}
					else if (tweenAction == TweenAction.GUI_SCALE)
					{
						LTRect ltRect2 = tween.ltRect;
						rect = tween.ltRect.rect;
						float x2 = rect.x;
						rect = tween.ltRect.rect;
						ltRect2.rect = new Rect(x2, rect.y, newVect.x, newVect.y);
					}
					else if (tweenAction == TweenAction.GUI_ALPHA)
					{
						tween.ltRect.alpha = newVect.x;
					}
					else if (tweenAction == TweenAction.GUI_ROTATE)
					{
						tween.ltRect.rotation = newVect.x;
					}
					else if (tweenAction == TweenAction.CANVAS_MOVE)
					{
						tween.rectTransform.anchoredPosition3D = newVect;
					}
					else if (tweenAction == TweenAction.CANVAS_SCALE)
					{
						((Transform)tween.rectTransform).localScale = newVect;
					}
				}
				if (dt != 0f && tween.hasUpdateCallback)
				{
					if (tween.onUpdateFloat != null)
					{
						tween.onUpdateFloat(LeanTween.val);
					}
					if (tween.onUpdateFloatRatio != null)
					{
						tween.onUpdateFloatRatio(LeanTween.val, ratioPassed);
					}
					else if (tween.onUpdateFloatObject != null)
					{
						tween.onUpdateFloatObject(LeanTween.val, tween.onUpdateParam);
					}
					else if (tween.onUpdateVector3Object != null)
					{
						tween.onUpdateVector3Object(newVect, tween.onUpdateParam);
					}
					else if (tween.onUpdateVector3 != null)
					{
						tween.onUpdateVector3(newVect);
					}
					else if (tween.onUpdateVector2 != null)
					{
						tween.onUpdateVector2(new Vector2(newVect.x, newVect.y));
					}
				}
			}
			if (isTweenFinished)
			{
				if (tween.loopType == LeanTweenType.once || tween.loopCount == 1)
				{
					tweensFinished[finishedCnt] = i;
					finishedCnt++;
					if (tweenAction == TweenAction.GUI_ROTATE)
					{
						tween.ltRect.rotateFinished = true;
					}
					if (tweenAction == TweenAction.DELAYED_SOUND)
					{
						AudioSource.PlayClipAtPoint((AudioClip)tween.onCompleteParam, tween.to, tween.from.x);
					}
					continue;
				}
				if ((tween.loopCount < 0 && tween.type == TweenAction.CALLBACK) || tween.onCompleteOnRepeat)
				{
					if (tweenAction == TweenAction.DELAYED_SOUND)
					{
						AudioSource.PlayClipAtPoint((AudioClip)tween.onCompleteParam, tween.to, tween.from.x);
					}
					if (tween.onComplete != null)
					{
						tween.onComplete();
					}
					else if (tween.onCompleteObject != null)
					{
						tween.onCompleteObject(tween.onCompleteParam);
					}
				}
				if (tween.loopCount >= 1)
				{
					tween.loopCount--;
				}
				if (tween.loopType == LeanTweenType.pingPong)
				{
					tween.direction = 0f - tween.direction;
				}
				else
				{
					tween.passed = Mathf.Epsilon;
				}
			}
			else if (tween.delay <= 0f)
			{
				tween.passed += dt * tween.direction;
			}
			else
			{
				tween.delay -= dt;
				if (tween.delay < 0f)
				{
					tween.passed = 0f;
					tween.delay = 0f;
				}
			}
		}
		tweenMaxSearch = maxTweenReached;
		frameRendered = Time.frameCount;
		for (int k = 0; k < finishedCnt; k++)
		{
			LeanTween.j = tweensFinished[k];
			tween = tweens[LeanTween.j];
			if (tween.onComplete != null)
			{
				Action onComplete = tween.onComplete;
				removeTween(LeanTween.j);
				onComplete();
			}
			else if (tween.onCompleteObject != null)
			{
				Action<object> onCompleteObject = tween.onCompleteObject;
				object onCompleteParam = tween.onCompleteParam;
				removeTween(LeanTween.j);
				onCompleteObject(onCompleteParam);
			}
			else
			{
				removeTween(LeanTween.j);
			}
		}
	}

	private static void alphaRecursive(Transform transform, float val, bool useRecursion = true)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Expected Obj, but got Unknown
		Renderer component = ((Component)transform).gameObject.GetComponent<Renderer>();
		if ((Object)(object)component != (Object)null)
		{
			Material[] materials = component.materials;
			foreach (Material val2 in materials)
			{
				if (val2.HasProperty("_Color"))
				{
					val2.color = new Color(val2.color.r, val2.color.g, val2.color.b, val);
				}
				else if (val2.HasProperty("_TintColor"))
				{
					Color val3 = val2.GetColor("_TintColor");
					val2.SetColor("_TintColor", new Color(val3.r, val3.g, val3.b, val));
				}
			}
		}
		if (!useRecursion || transform.childCount <= 0)
		{
			return;
		}
		foreach (Transform item in transform)
		{
			alphaRecursive(item, val);
		}
	}

	private static void colorRecursive(Transform transform, Color toColor, bool useRecursion = true)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected Obj, but got Unknown
		Renderer component = ((Component)transform).gameObject.GetComponent<Renderer>();
		if ((Object)(object)component != (Object)null)
		{
			Material[] materials = component.materials;
			for (int i = 0; i < materials.Length; i++)
			{
				materials[i].color = toColor;
			}
		}
		if (!useRecursion || transform.childCount <= 0)
		{
			return;
		}
		foreach (Transform item in transform)
		{
			colorRecursive(item, toColor);
		}
	}

	private static void alphaRecursive(RectTransform rectTransform, float val, int recursiveLevel = 0)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected Obj, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (((Transform)rectTransform).childCount <= 0)
		{
			return;
		}
		foreach (RectTransform item in (Transform)rectTransform)
		{
			Image component = ((Component)item).GetComponent<Image>();
			if ((Object)(object)component != (Object)null)
			{
				Color val3 = ((Graphic)component).color;
				val3.a = val;
				((Graphic)component).color = val3;
			}
			alphaRecursive(item, val, recursiveLevel + 1);
		}
	}

	private static void alphaRecursiveSprite(Transform transform, float val)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected Obj, but got Unknown
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (transform.childCount <= 0)
		{
			return;
		}
		foreach (Transform item in transform)
		{
			SpriteRenderer component = ((Component)item).GetComponent<SpriteRenderer>();
			if ((Object)(object)component != (Object)null)
			{
				component.color = new Color(component.color.r, component.color.g, component.color.b, val);
			}
			alphaRecursiveSprite(item, val);
		}
	}

	private static void colorRecursiveSprite(Transform transform, Color toColor)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected Obj, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (transform.childCount <= 0)
		{
			return;
		}
		foreach (Transform item in transform)
		{
			SpriteRenderer component = ((Component)trans).gameObject.GetComponent<SpriteRenderer>();
			if ((Object)(object)component != (Object)null)
			{
				component.color = toColor;
			}
			colorRecursiveSprite(item, toColor);
		}
	}

	private static void colorRecursive(RectTransform rectTransform, Color toColor)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected Obj, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (((Transform)rectTransform).childCount <= 0)
		{
			return;
		}
		foreach (RectTransform item in (Transform)rectTransform)
		{
			Image component = ((Component)item).GetComponent<Image>();
			if ((Object)(object)component != (Object)null)
			{
				((Graphic)component).color = toColor;
			}
			colorRecursive(item, toColor);
		}
	}

	private static void textAlphaRecursive(Transform trans, float val, bool useRecursion = true)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected Obj, but got Unknown
		Text component = ((Component)trans).gameObject.GetComponent<Text>();
		if ((Object)(object)component != (Object)null)
		{
			Color val2 = ((Graphic)component).color;
			val2.a = val;
			((Graphic)component).color = val2;
		}
		if (!useRecursion || trans.childCount <= 0)
		{
			return;
		}
		foreach (Transform tran in trans)
		{
			textAlphaRecursive(tran, val);
		}
	}

	private static void textColorRecursive(Transform trans, Color toColor)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected Obj, but got Unknown
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (trans.childCount <= 0)
		{
			return;
		}
		foreach (Transform tran in trans)
		{
			Text component = ((Component)tran).gameObject.GetComponent<Text>();
			if ((Object)(object)component != (Object)null)
			{
				((Graphic)component).color = toColor;
			}
			textColorRecursive(tran, toColor);
		}
	}

	private static Color tweenColor(LTDescrImpl tween, float val)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val2 = tween.point - tween.axis;
		float num = tween.to.y - tween.from.y;
		return new Color(tween.axis.x + val2.x * val, tween.axis.y + val2.y * val, tween.axis.z + val2.z * val, tween.from.y + num * val);
	}

	public static void removeTween(int i, int uniqueId)
	{
		if (tweens[i].uniqueId == uniqueId)
		{
			removeTween(i);
		}
	}

	public static void removeTween(int i)
	{
		if (!tweens[i].toggle)
		{
			return;
		}
		tweens[i].toggle = false;
		if (tweens[i].destroyOnComplete)
		{
			if (tweens[i].ltRect != null)
			{
				LTGUI.destroy(tweens[i].ltRect.id);
			}
			else if ((Object)(object)tweens[i].trans != (Object)null && (Object)(object)((Component)tweens[i].trans).gameObject != (Object)(object)_tweenEmpty)
			{
				Object.Destroy((Object)(object)((Component)tweens[i].trans).gameObject);
			}
		}
		tweens[i].cleanup();
		startSearch = i;
		if (i + 1 >= tweenMaxSearch)
		{
			startSearch = 0;
		}
	}

	public static Vector3[] add(Vector3[] a, Vector3 b)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		Vector3[] array = new Vector3[a.Length];
		for (i = 0; i < a.Length; i++)
		{
			array[i] = a[i] + b;
		}
		return array;
	}

	public static float closestRot(float from, float to)
	{
		float num = 0f - (360f - to);
		float num2 = 360f + to;
		float num3 = Mathf.Abs(to - from);
		float num4 = Mathf.Abs(num - from);
		float num5 = Mathf.Abs(num2 - from);
		if (num3 < num4 && num3 < num5)
		{
			return to;
		}
		if (num4 < num5)
		{
			return num;
		}
		return num2;
	}

	public static void cancelAll()
	{
		cancelAll(callComplete: false);
	}

	public static void cancelAll(bool callComplete)
	{
		init();
		for (int i = 0; i <= tweenMaxSearch; i++)
		{
			if ((Object)(object)tweens[i].trans != (Object)null)
			{
				if (callComplete && tweens[i].onComplete != null)
				{
					tweens[i].onComplete();
				}
				removeTween(i);
			}
		}
	}

	public static void cancel(GameObject gameObject)
	{
		cancel(gameObject, callOnComplete: false);
	}

	public static void cancel(GameObject gameObject, bool callOnComplete)
	{
		init();
		Transform transform = gameObject.transform;
		for (int i = 0; i <= tweenMaxSearch; i++)
		{
			if (tweens[i].toggle && (Object)(object)tweens[i].trans == (Object)(object)transform)
			{
				if (callOnComplete && tweens[i].onComplete != null)
				{
					tweens[i].onComplete();
				}
				removeTween(i);
			}
		}
	}

	public static void cancel(GameObject gameObject, int uniqueId)
	{
		if (uniqueId >= 0)
		{
			init();
			int num = uniqueId & 0xFFFF;
			int num2 = uniqueId >> 16;
			if ((Object)(object)tweens[num].trans == (Object)null || ((Object)(object)((Component)tweens[num].trans).gameObject == (Object)(object)gameObject && tweens[num].counter == num2))
			{
				removeTween(num);
			}
		}
	}

	public static void cancel(LTRect ltRect, int uniqueId)
	{
		if (uniqueId >= 0)
		{
			init();
			int num = uniqueId & 0xFFFF;
			int num2 = uniqueId >> 16;
			if (tweens[num].ltRect == ltRect && tweens[num].counter == num2)
			{
				removeTween(num);
			}
		}
	}

	public static void cancel(int uniqueId)
	{
		cancel(uniqueId, callOnComplete: false);
	}

	public static void cancel(int uniqueId, bool callOnComplete)
	{
		if (uniqueId < 0)
		{
			return;
		}
		init();
		int num = uniqueId & 0xFFFF;
		int num2 = uniqueId >> 16;
		if (tweens[num].counter == num2)
		{
			if (callOnComplete && tweens[num].onComplete != null)
			{
				tweens[num].onComplete();
			}
			removeTween(num);
		}
	}

	public static LTDescr descr(int uniqueId)
	{
		int num = uniqueId & 0xFFFF;
		int num2 = uniqueId >> 16;
		if (tweens[num] != null && tweens[num].uniqueId == uniqueId && tweens[num].counter == num2)
		{
			return tweens[num];
		}
		for (int i = 0; i <= tweenMaxSearch; i++)
		{
			if (tweens[i].uniqueId == uniqueId && tweens[i].counter == num2)
			{
				return tweens[i];
			}
		}
		return null;
	}

	public static LTDescr description(int uniqueId)
	{
		return descr(uniqueId);
	}

	public static LTDescr[] descriptions(GameObject gameObject = null)
	{
		if ((Object)(object)gameObject == (Object)null)
		{
			return null;
		}
		List<LTDescr> list = new List<LTDescr>();
		Transform transform = gameObject.transform;
		for (int i = 0; i <= tweenMaxSearch; i++)
		{
			if (tweens[i].toggle && (Object)(object)tweens[i].trans == (Object)(object)transform)
			{
				list.Add(tweens[i]);
			}
		}
		return list.ToArray();
	}

	[Obsolete("Use 'pause( id )' instead")]
	public static void pause(GameObject gameObject, int uniqueId)
	{
		pause(uniqueId);
	}

	public static void pause(int uniqueId)
	{
		int num = uniqueId & 0xFFFF;
		int num2 = uniqueId >> 16;
		if (tweens[num].counter == num2)
		{
			tweens[num].pause();
		}
	}

	public static void pause(GameObject gameObject)
	{
		Transform transform = gameObject.transform;
		for (int i = 0; i <= tweenMaxSearch; i++)
		{
			if ((Object)(object)tweens[i].trans == (Object)(object)transform)
			{
				tweens[i].pause();
			}
		}
	}

	public static void pauseAll()
	{
		init();
		for (int i = 0; i <= tweenMaxSearch; i++)
		{
			tweens[i].pause();
		}
	}

	public static void resumeAll()
	{
		init();
		for (int i = 0; i <= tweenMaxSearch; i++)
		{
			tweens[i].resume();
		}
	}

	[Obsolete("Use 'resume( id )' instead")]
	public static void resume(GameObject gameObject, int uniqueId)
	{
		resume(uniqueId);
	}

	public static void resume(int uniqueId)
	{
		int num = uniqueId & 0xFFFF;
		int num2 = uniqueId >> 16;
		if (tweens[num].counter == num2)
		{
			tweens[num].resume();
		}
	}

	public static void resume(GameObject gameObject)
	{
		Transform transform = gameObject.transform;
		for (int i = 0; i <= tweenMaxSearch; i++)
		{
			if ((Object)(object)tweens[i].trans == (Object)(object)transform)
			{
				tweens[i].resume();
			}
		}
	}

	public static bool isTweening(GameObject gameObject = null)
	{
		if ((Object)(object)gameObject == (Object)null)
		{
			for (int i = 0; i <= tweenMaxSearch; i++)
			{
				if (tweens[i].toggle)
				{
					return true;
				}
			}
			return false;
		}
		Transform transform = gameObject.transform;
		for (int j = 0; j <= tweenMaxSearch; j++)
		{
			if (tweens[j].toggle && (Object)(object)tweens[j].trans == (Object)(object)transform)
			{
				return true;
			}
		}
		return false;
	}

	public static bool isTweening(int uniqueId)
	{
		int num = uniqueId & 0xFFFF;
		int num2 = uniqueId >> 16;
		if (num < 0 || num >= maxTweens)
		{
			return false;
		}
		if (tweens[num].counter == num2 && tweens[num].toggle)
		{
			return true;
		}
		return false;
	}

	public static bool isTweening(LTRect ltRect)
	{
		for (int i = 0; i <= tweenMaxSearch; i++)
		{
			if (tweens[i].toggle && tweens[i].ltRect == ltRect)
			{
				return true;
			}
		}
		return false;
	}

	public static void drawBezierPath(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float arrowSize = 0f, Transform arrowTransform = null)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = a;
		Vector3 val2 = -a + 3f * (b - c) + d;
		Vector3 val3 = 3f * (a + c) - 6f * b;
		Vector3 val4 = 3f * (b - a);
		if (arrowSize > 0f)
		{
			Vector3 position = arrowTransform.position;
			Quaternion rotation = arrowTransform.rotation;
			float num = 0f;
			for (float num2 = 1f; num2 <= 120f; num2++)
			{
				float num3 = num2 / 120f;
				Vector3 val5 = ((val2 * num3 + val3) * num3 + val4) * num3 + a;
				Gizmos.DrawLine(val, val5);
				float num4 = num;
				Vector3 val6 = val5 - val;
				num = num4 + val6.magnitude;
				if (num > 1f)
				{
					num--;
					arrowTransform.position = val5;
					arrowTransform.LookAt(val, Vector3.forward);
					Vector3 val7 = arrowTransform.TransformDirection(Vector3.right);
					Vector3 val8 = val - val5;
					val8 = val8.normalized;
					Gizmos.DrawLine(val5, val5 + (val7 + val8) * arrowSize);
					val7 = arrowTransform.TransformDirection(-Vector3.right);
					Gizmos.DrawLine(val5, val5 + (val7 + val8) * arrowSize);
				}
				val = val5;
			}
			arrowTransform.position = position;
			arrowTransform.rotation = rotation;
		}
		else
		{
			for (float num5 = 1f; num5 <= 30f; num5++)
			{
				float num3 = num5 / 30f;
				Vector3 val5 = ((val2 * num3 + val3) * num3 + val4) * num3 + a;
				Gizmos.DrawLine(val, val5);
				val = val5;
			}
		}
	}

	public static object logError(string error)
	{
		if (throwErrors)
		{
			Debug.LogError((object)error);
		}
		else
		{
			Debug.Log((object)error);
		}
		return null;
	}

	public static LTDescr options(LTDescr seed)
	{
		Debug.LogError((object)"error this function is no longer used");
		return null;
	}

	public static LTDescr options()
	{
		init();
		bool flag = false;
		j = 0;
		i = startSearch;
		while (j < maxTweens)
		{
			if (i >= maxTweens - 1)
			{
				i = 0;
			}
			if (!tweens[i].toggle)
			{
				if (i + 1 > tweenMaxSearch)
				{
					tweenMaxSearch = i + 1;
				}
				startSearch = i + 1;
				flag = true;
				break;
			}
			j++;
			if (j >= maxTweens)
			{
				return logError("LeanTween - You have run out of available spaces for tweening. To avoid this error increase the number of spaces to available for tweening when you initialize the LeanTween class ex: LeanTween.init( " + maxTweens * 2 + " );") as LTDescr;
			}
			i++;
		}
		if (!flag)
		{
			logError("no available tween found!");
		}
		tweens[i].reset();
		tweens[i].setId((uint)i);
		return tweens[i];
	}

	private static LTDescr pushNewTween(GameObject gameObject, Vector3 to, float time, TweenAction tweenAction, LTDescr tween)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		init(maxTweens);
		if ((Object)(object)gameObject == (Object)null || tween == null)
		{
			return null;
		}
		tween.trans = gameObject.transform;
		tween.to = to;
		tween.time = time;
		tween.type = tweenAction;
		return tween;
	}

	public static LTDescr play(RectTransform rectTransform, Sprite[] sprites)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		float time = 0.25f * (float)sprites.Length;
		return pushNewTween(((Component)rectTransform).gameObject, new Vector3((float)sprites.Length - 1f, 0f, 0f), time, TweenAction.CANVAS_PLAYSPRITE, options().setSprites(sprites).setRepeat(-1));
	}

	public static LTDescr alpha(GameObject gameObject, float to, float time)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		LTDescr lTDescr = pushNewTween(gameObject, new Vector3(to, 0f, 0f), time, TweenAction.ALPHA, options());
		SpriteRenderer component = gameObject.GetComponent<SpriteRenderer>();
		lTDescr.spriteRen = component;
		return lTDescr;
	}

	public static LTDescr alpha(LTRect ltRect, float to, float time)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		ltRect.alphaEnabled = true;
		return pushNewTween(tweenEmpty, new Vector3(to, 0f, 0f), time, TweenAction.GUI_ALPHA, options().setRect(ltRect));
	}

	public static LTDescr textAlpha(RectTransform rectTransform, float to, float time)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(((Component)rectTransform).gameObject, new Vector3(to, 0f, 0f), time, TweenAction.TEXT_ALPHA, options());
	}

	public static LTDescr alphaText(RectTransform rectTransform, float to, float time)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(((Component)rectTransform).gameObject, new Vector3(to, 0f, 0f), time, TweenAction.TEXT_ALPHA, options());
	}

	public static LTDescr alphaCanvas(CanvasGroup canvasGroup, float to, float time)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(((Component)canvasGroup).gameObject, new Vector3(to, 0f, 0f), time, TweenAction.CANVASGROUP_ALPHA, options());
	}

	public static LTDescr alphaVertex(GameObject gameObject, float to, float time)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(gameObject, new Vector3(to, 0f, 0f), time, TweenAction.ALPHA_VERTEX, options());
	}

	public static LTDescr color(GameObject gameObject, Color to, float time)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		LTDescr lTDescr = pushNewTween(gameObject, new Vector3(1f, to.a, 0f), time, TweenAction.COLOR, options().setPoint(new Vector3(to.r, to.g, to.b)));
		SpriteRenderer component = gameObject.GetComponent<SpriteRenderer>();
		lTDescr.spriteRen = component;
		return lTDescr;
	}

	public static LTDescr textColor(RectTransform rectTransform, Color to, float time)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(((Component)rectTransform).gameObject, new Vector3(1f, to.a, 0f), time, TweenAction.TEXT_COLOR, options().setPoint(new Vector3(to.r, to.g, to.b)));
	}

	public static LTDescr colorText(RectTransform rectTransform, Color to, float time)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(((Component)rectTransform).gameObject, new Vector3(1f, to.a, 0f), time, TweenAction.TEXT_COLOR, options().setPoint(new Vector3(to.r, to.g, to.b)));
	}

	public static LTDescr delayedCall(float delayTime, Action callback)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(tweenEmpty, Vector3.zero, delayTime, TweenAction.CALLBACK, options().setOnComplete(callback));
	}

	public static LTDescr delayedCall(float delayTime, Action<object> callback)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(tweenEmpty, Vector3.zero, delayTime, TweenAction.CALLBACK, options().setOnComplete(callback));
	}

	public static LTDescr delayedCall(GameObject gameObject, float delayTime, Action callback)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(gameObject, Vector3.zero, delayTime, TweenAction.CALLBACK, options().setOnComplete(callback));
	}

	public static LTDescr delayedCall(GameObject gameObject, float delayTime, Action<object> callback)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(gameObject, Vector3.zero, delayTime, TweenAction.CALLBACK, options().setOnComplete(callback));
	}

	public static LTDescr destroyAfter(LTRect rect, float delayTime)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(tweenEmpty, Vector3.zero, delayTime, TweenAction.CALLBACK, options().setRect(rect).setDestroyOnComplete(doesDestroy: true));
	}

	public static LTDescr move(GameObject gameObject, Vector3 to, float time)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(gameObject, to, time, TweenAction.MOVE, options());
	}

	public static LTDescr move(GameObject gameObject, Vector2 to, float time)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(gameObject, new Vector3(to.x, to.y, gameObject.transform.position.z), time, TweenAction.MOVE, options());
	}

	public static LTDescr move(GameObject gameObject, Vector3[] to, float time)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		d = options();
		if (d.path == null)
		{
			d.path = new LTBezierPath(to);
		}
		else
		{
			d.path.setPoints(to);
		}
		return pushNewTween(gameObject, new Vector3(1f, 0f, 0f), time, TweenAction.MOVE_CURVED, d);
	}

	public static LTDescr move(GameObject gameObject, LTBezierPath to, float time)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		d = options();
		d.path = to;
		return pushNewTween(gameObject, new Vector3(1f, 0f, 0f), time, TweenAction.MOVE_CURVED, d);
	}

	public static LTDescr move(GameObject gameObject, LTSpline to, float time)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		d = options();
		d.spline = to;
		return pushNewTween(gameObject, new Vector3(1f, 0f, 0f), time, TweenAction.MOVE_SPLINE, d);
	}

	public static LTDescr moveSpline(GameObject gameObject, Vector3[] to, float time)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		d = options();
		d.spline = new LTSpline(to);
		return pushNewTween(gameObject, new Vector3(1f, 0f, 0f), time, TweenAction.MOVE_SPLINE, d);
	}

	public static LTDescr moveSpline(GameObject gameObject, LTSpline to, float time)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		d = options();
		d.spline = to;
		return pushNewTween(gameObject, new Vector3(1f, 0f, 0f), time, TweenAction.MOVE_SPLINE, d);
	}

	public static LTDescr moveSplineLocal(GameObject gameObject, Vector3[] to, float time)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		d = options();
		d.spline = new LTSpline(to);
		return pushNewTween(gameObject, new Vector3(1f, 0f, 0f), time, TweenAction.MOVE_SPLINE_LOCAL, d);
	}

	public static LTDescr move(LTRect ltRect, Vector2 to, float time)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(tweenEmpty, Vector2.op_Implicit(to), time, TweenAction.GUI_MOVE, options().setRect(ltRect));
	}

	public static LTDescr moveMargin(LTRect ltRect, Vector2 to, float time)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(tweenEmpty, Vector2.op_Implicit(to), time, TweenAction.GUI_MOVE_MARGIN, options().setRect(ltRect));
	}

	public static LTDescr moveX(GameObject gameObject, float to, float time)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(gameObject, new Vector3(to, 0f, 0f), time, TweenAction.MOVE_X, options());
	}

	public static LTDescr moveY(GameObject gameObject, float to, float time)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(gameObject, new Vector3(to, 0f, 0f), time, TweenAction.MOVE_Y, options());
	}

	public static LTDescr moveZ(GameObject gameObject, float to, float time)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(gameObject, new Vector3(to, 0f, 0f), time, TweenAction.MOVE_Z, options());
	}

	public static LTDescr moveLocal(GameObject gameObject, Vector3 to, float time)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(gameObject, to, time, TweenAction.MOVE_LOCAL, options());
	}

	public static LTDescr moveLocal(GameObject gameObject, Vector3[] to, float time)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		d = options();
		if (d.path == null)
		{
			d.path = new LTBezierPath(to);
		}
		else
		{
			d.path.setPoints(to);
		}
		return pushNewTween(gameObject, new Vector3(1f, 0f, 0f), time, TweenAction.MOVE_CURVED_LOCAL, d);
	}

	public static LTDescr moveLocalX(GameObject gameObject, float to, float time)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(gameObject, new Vector3(to, 0f, 0f), time, TweenAction.MOVE_LOCAL_X, options());
	}

	public static LTDescr moveLocalY(GameObject gameObject, float to, float time)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(gameObject, new Vector3(to, 0f, 0f), time, TweenAction.MOVE_LOCAL_Y, options());
	}

	public static LTDescr moveLocalZ(GameObject gameObject, float to, float time)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(gameObject, new Vector3(to, 0f, 0f), time, TweenAction.MOVE_LOCAL_Z, options());
	}

	public static LTDescr moveLocal(GameObject gameObject, LTBezierPath to, float time)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		d = options();
		d.path = to;
		return pushNewTween(gameObject, new Vector3(1f, 0f, 0f), time, TweenAction.MOVE_CURVED_LOCAL, d);
	}

	public static LTDescr moveLocal(GameObject gameObject, LTSpline to, float time)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		d = options();
		d.spline = to;
		return pushNewTween(gameObject, new Vector3(1f, 0f, 0f), time, TweenAction.MOVE_SPLINE_LOCAL, d);
	}

	public static LTDescr move(GameObject gameObject, Transform to, float time)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(gameObject, Vector3.zero, time, TweenAction.MOVE_TO_TRANSFORM, options().setTo(to));
	}

	public static LTDescr rotate(GameObject gameObject, Vector3 to, float time)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(gameObject, to, time, TweenAction.ROTATE, options());
	}

	public static LTDescr rotate(LTRect ltRect, float to, float time)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(tweenEmpty, new Vector3(to, 0f, 0f), time, TweenAction.GUI_ROTATE, options().setRect(ltRect));
	}

	public static LTDescr rotateLocal(GameObject gameObject, Vector3 to, float time)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(gameObject, to, time, TweenAction.ROTATE_LOCAL, options());
	}

	public static LTDescr rotateX(GameObject gameObject, float to, float time)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(gameObject, new Vector3(to, 0f, 0f), time, TweenAction.ROTATE_X, options());
	}

	public static LTDescr rotateY(GameObject gameObject, float to, float time)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(gameObject, new Vector3(to, 0f, 0f), time, TweenAction.ROTATE_Y, options());
	}

	public static LTDescr rotateZ(GameObject gameObject, float to, float time)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(gameObject, new Vector3(to, 0f, 0f), time, TweenAction.ROTATE_Z, options());
	}

	public static LTDescr rotateAround(GameObject gameObject, Vector3 axis, float add, float time)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(gameObject, new Vector3(add, 0f, 0f), time, TweenAction.ROTATE_AROUND, options().setAxis(axis));
	}

	public static LTDescr rotateAroundLocal(GameObject gameObject, Vector3 axis, float add, float time)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(gameObject, new Vector3(add, 0f, 0f), time, TweenAction.ROTATE_AROUND_LOCAL, options().setAxis(axis));
	}

	public static LTDescr scale(GameObject gameObject, Vector3 to, float time)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(gameObject, to, time, TweenAction.SCALE, options());
	}

	public static LTDescr scale(LTRect ltRect, Vector2 to, float time)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(tweenEmpty, Vector2.op_Implicit(to), time, TweenAction.GUI_SCALE, options().setRect(ltRect));
	}

	public static LTDescr scaleX(GameObject gameObject, float to, float time)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(gameObject, new Vector3(to, 0f, 0f), time, TweenAction.SCALE_X, options());
	}

	public static LTDescr scaleY(GameObject gameObject, float to, float time)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(gameObject, new Vector3(to, 0f, 0f), time, TweenAction.SCALE_Y, options());
	}

	public static LTDescr scaleZ(GameObject gameObject, float to, float time)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(gameObject, new Vector3(to, 0f, 0f), time, TweenAction.SCALE_Z, options());
	}

	public static LTDescr value(GameObject gameObject, float from, float to, float time)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(gameObject, new Vector3(to, 0f, 0f), time, TweenAction.CALLBACK, options().setFrom(new Vector3(from, 0f, 0f)));
	}

	public static LTDescr value(GameObject gameObject, Vector2 from, Vector2 to, float time)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(gameObject, new Vector3(to.x, to.y, 0f), time, TweenAction.VALUE3, options().setTo(new Vector3(to.x, to.y, 0f)).setFrom(new Vector3(from.x, from.y, 0f)));
	}

	public static LTDescr value(GameObject gameObject, Vector3 from, Vector3 to, float time)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(gameObject, to, time, TweenAction.VALUE3, options().setFrom(from));
	}

	public static LTDescr value(GameObject gameObject, Color from, Color to, float time)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		LTDescr lTDescr = pushNewTween(gameObject, new Vector3(1f, to.a, 0f), time, TweenAction.CALLBACK_COLOR, options().setPoint(new Vector3(to.r, to.g, to.b)).setFromColor(from).setHasInitialized(has: false));
		SpriteRenderer component = gameObject.GetComponent<SpriteRenderer>();
		lTDescr.spriteRen = component;
		return lTDescr;
	}

	public static LTDescr value(GameObject gameObject, Action<float> callOnUpdate, float from, float to, float time)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(gameObject, new Vector3(to, 0f, 0f), time, TweenAction.CALLBACK, options().setTo(new Vector3(to, 0f, 0f)).setFrom(new Vector3(from, 0f, 0f)).setOnUpdate(callOnUpdate));
	}

	public static LTDescr value(GameObject gameObject, Action<float, float> callOnUpdateRatio, float from, float to, float time)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(gameObject, new Vector3(to, 0f, 0f), time, TweenAction.CALLBACK, options().setTo(new Vector3(to, 0f, 0f)).setFrom(new Vector3(from, 0f, 0f)).setOnUpdateRatio(callOnUpdateRatio));
	}

	public static LTDescr value(GameObject gameObject, Action<Color> callOnUpdate, Color from, Color to, float time)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(gameObject, new Vector3(1f, to.a, 0f), time, TweenAction.CALLBACK_COLOR, options().setPoint(new Vector3(to.r, to.g, to.b)).setAxis(new Vector3(from.r, from.g, from.b)).setFrom(new Vector3(0f, from.a, 0f))
			.setHasInitialized(has: false)
			.setOnUpdateColor(callOnUpdate));
	}

	public static LTDescr value(GameObject gameObject, Action<Color, object> callOnUpdate, Color from, Color to, float time)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(gameObject, new Vector3(1f, to.a, 0f), time, TweenAction.CALLBACK_COLOR, options().setPoint(new Vector3(to.r, to.g, to.b)).setAxis(new Vector3(from.r, from.g, from.b)).setFrom(new Vector3(0f, from.a, 0f))
			.setHasInitialized(has: false)
			.setOnUpdateColor(callOnUpdate));
	}

	public static LTDescr value(GameObject gameObject, Action<Vector2> callOnUpdate, Vector2 from, Vector2 to, float time)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(gameObject, new Vector3(to.x, to.y, 0f), time, TweenAction.VALUE3, options().setTo(new Vector3(to.x, to.y, 0f)).setFrom(new Vector3(from.x, from.y, 0f)).setOnUpdateVector2(callOnUpdate));
	}

	public static LTDescr value(GameObject gameObject, Action<Vector3> callOnUpdate, Vector3 from, Vector3 to, float time)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(gameObject, to, time, TweenAction.VALUE3, options().setTo(to).setFrom(from).setOnUpdateVector3(callOnUpdate));
	}

	public static LTDescr value(GameObject gameObject, Action<float, object> callOnUpdate, float from, float to, float time)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(gameObject, new Vector3(to, 0f, 0f), time, TweenAction.CALLBACK, options().setTo(new Vector3(to, 0f, 0f)).setFrom(new Vector3(from, 0f, 0f)).setOnUpdate(callOnUpdate, gameObject));
	}

	public static LTDescr delayedSound(AudioClip audio, Vector3 pos, float volume)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(tweenEmpty, pos, 0f, TweenAction.DELAYED_SOUND, options().setTo(pos).setFrom(new Vector3(volume, 0f, 0f)).setAudio(audio));
	}

	public static LTDescr delayedSound(GameObject gameObject, AudioClip audio, Vector3 pos, float volume)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(gameObject, pos, 0f, TweenAction.DELAYED_SOUND, options().setTo(pos).setFrom(new Vector3(volume, 0f, 0f)).setAudio(audio));
	}

	public static LTDescr move(RectTransform rectTrans, Vector3 to, float time)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(((Component)rectTrans).gameObject, to, time, TweenAction.CANVAS_MOVE, options().setRect(rectTrans));
	}

	public static LTDescr moveX(RectTransform rectTrans, float to, float time)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(((Component)rectTrans).gameObject, new Vector3(to, 0f, 0f), time, TweenAction.CANVAS_MOVE_X, options().setRect(rectTrans));
	}

	public static LTDescr moveY(RectTransform rectTrans, float to, float time)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(((Component)rectTrans).gameObject, new Vector3(to, 0f, 0f), time, TweenAction.CANVAS_MOVE_Y, options().setRect(rectTrans));
	}

	public static LTDescr moveZ(RectTransform rectTrans, float to, float time)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(((Component)rectTrans).gameObject, new Vector3(to, 0f, 0f), time, TweenAction.CANVAS_MOVE_Z, options().setRect(rectTrans));
	}

	public static LTDescr rotate(RectTransform rectTrans, float to, float time)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(((Component)rectTrans).gameObject, new Vector3(to, 0f, 0f), time, TweenAction.CANVAS_ROTATEAROUND, options().setRect(rectTrans).setAxis(Vector3.forward));
	}

	public static LTDescr rotateAround(RectTransform rectTrans, Vector3 axis, float to, float time)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(((Component)rectTrans).gameObject, new Vector3(to, 0f, 0f), time, TweenAction.CANVAS_ROTATEAROUND, options().setRect(rectTrans).setAxis(axis));
	}

	public static LTDescr rotateAroundLocal(RectTransform rectTrans, Vector3 axis, float to, float time)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(((Component)rectTrans).gameObject, new Vector3(to, 0f, 0f), time, TweenAction.CANVAS_ROTATEAROUND_LOCAL, options().setRect(rectTrans).setAxis(axis));
	}

	public static LTDescr scale(RectTransform rectTrans, Vector3 to, float time)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(((Component)rectTrans).gameObject, to, time, TweenAction.CANVAS_SCALE, options().setRect(rectTrans));
	}

	public static LTDescr alpha(RectTransform rectTrans, float to, float time)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(((Component)rectTrans).gameObject, new Vector3(to, 0f, 0f), time, TweenAction.CANVAS_ALPHA, options().setRect(rectTrans));
	}

	public static LTDescr color(RectTransform rectTrans, Color to, float time)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		return pushNewTween(((Component)rectTrans).gameObject, new Vector3(1f, to.a, 0f), time, TweenAction.CANVAS_COLOR, options().setRect(rectTrans).setPoint(new Vector3(to.r, to.g, to.b)));
	}

	private static float tweenOnCurve(LTDescrImpl tweenDescr, float ratioPassed)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return tweenDescr.from.x + tweenDescr.diff.x * tweenDescr.animationCurve.Evaluate(ratioPassed);
	}

	private static Vector3 tweenOnCurveVector(LTDescrImpl tweenDescr, float ratioPassed)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(tweenDescr.from.x + tweenDescr.diff.x * tweenDescr.animationCurve.Evaluate(ratioPassed), tweenDescr.from.y + tweenDescr.diff.y * tweenDescr.animationCurve.Evaluate(ratioPassed), tweenDescr.from.z + tweenDescr.diff.z * tweenDescr.animationCurve.Evaluate(ratioPassed));
	}

	private static float easeOutQuadOpt(float start, float diff, float ratioPassed)
	{
		return (0f - diff) * ratioPassed * (ratioPassed - 2f) + start;
	}

	private static float easeInQuadOpt(float start, float diff, float ratioPassed)
	{
		return diff * ratioPassed * ratioPassed + start;
	}

	private static float easeInOutQuadOpt(float start, float diff, float ratioPassed)
	{
		ratioPassed /= 0.5f;
		if (ratioPassed < 1f)
		{
			return diff / 2f * ratioPassed * ratioPassed + start;
		}
		ratioPassed--;
		return (0f - diff) / 2f * (ratioPassed * (ratioPassed - 2f) - 1f) + start;
	}

	private static float linear(float start, float end, float val)
	{
		return Mathf.Lerp(start, end, val);
	}

	private static float clerp(float start, float end, float val)
	{
		float num = 0f;
		float num2 = 360f;
		float num3 = Mathf.Abs((num2 - num) / 2f);
		float num4 = 0f;
		float num5 = 0f;
		if (end - start < 0f - num3)
		{
			num5 = (num2 - start + end) * val;
			return start + num5;
		}
		if (end - start > num3)
		{
			num5 = (0f - (num2 - end + start)) * val;
			return start + num5;
		}
		return start + (end - start) * val;
	}

	private static float spring(float start, float end, float val)
	{
		val = Mathf.Clamp01(val);
		val = (Mathf.Sin(val * (float)Math.PI * (0.2f + 2.5f * val * val * val)) * Mathf.Pow(1f - val, 2.2f) + val) * (1f + 1.2f * (1f - val));
		return start + (end - start) * val;
	}

	private static float easeInQuad(float start, float end, float val)
	{
		end -= start;
		return end * val * val + start;
	}

	private static float easeOutQuad(float start, float end, float val)
	{
		end -= start;
		return (0f - end) * val * (val - 2f) + start;
	}

	private static float easeInOutQuad(float start, float end, float val)
	{
		val /= 0.5f;
		end -= start;
		if (val < 1f)
		{
			return end / 2f * val * val + start;
		}
		val--;
		return (0f - end) / 2f * (val * (val - 2f) - 1f) + start;
	}

	private static float easeInCubic(float start, float end, float val)
	{
		end -= start;
		return end * val * val * val + start;
	}

	private static float easeOutCubic(float start, float end, float val)
	{
		val--;
		end -= start;
		return end * (val * val * val + 1f) + start;
	}

	private static float easeInOutCubic(float start, float end, float val)
	{
		val /= 0.5f;
		end -= start;
		if (val < 1f)
		{
			return end / 2f * val * val * val + start;
		}
		val -= 2f;
		return end / 2f * (val * val * val + 2f) + start;
	}

	private static float easeInQuart(float start, float end, float val)
	{
		end -= start;
		return end * val * val * val * val + start;
	}

	private static float easeOutQuart(float start, float end, float val)
	{
		val--;
		end -= start;
		return (0f - end) * (val * val * val * val - 1f) + start;
	}

	private static float easeInOutQuart(float start, float end, float val)
	{
		val /= 0.5f;
		end -= start;
		if (val < 1f)
		{
			return end / 2f * val * val * val * val + start;
		}
		val -= 2f;
		return (0f - end) / 2f * (val * val * val * val - 2f) + start;
	}

	private static float easeInQuint(float start, float end, float val)
	{
		end -= start;
		return end * val * val * val * val * val + start;
	}

	private static float easeOutQuint(float start, float end, float val)
	{
		val--;
		end -= start;
		return end * (val * val * val * val * val + 1f) + start;
	}

	private static float easeInOutQuint(float start, float end, float val)
	{
		val /= 0.5f;
		end -= start;
		if (val < 1f)
		{
			return end / 2f * val * val * val * val * val + start;
		}
		val -= 2f;
		return end / 2f * (val * val * val * val * val + 2f) + start;
	}

	private static float easeInSine(float start, float end, float val)
	{
		end -= start;
		return (0f - end) * Mathf.Cos(val / 1f * ((float)Math.PI / 2f)) + end + start;
	}

	private static float easeOutSine(float start, float end, float val)
	{
		end -= start;
		return end * Mathf.Sin(val / 1f * ((float)Math.PI / 2f)) + start;
	}

	private static float easeInOutSine(float start, float end, float val)
	{
		end -= start;
		return (0f - end) / 2f * (Mathf.Cos((float)Math.PI * val / 1f) - 1f) + start;
	}

	private static float easeInExpo(float start, float end, float val)
	{
		end -= start;
		return end * Mathf.Pow(2f, 10f * (val / 1f - 1f)) + start;
	}

	private static float easeOutExpo(float start, float end, float val)
	{
		end -= start;
		return end * (0f - Mathf.Pow(2f, -10f * val / 1f) + 1f) + start;
	}

	private static float easeInOutExpo(float start, float end, float val)
	{
		val /= 0.5f;
		end -= start;
		if (val < 1f)
		{
			return end / 2f * Mathf.Pow(2f, 10f * (val - 1f)) + start;
		}
		val--;
		return end / 2f * (0f - Mathf.Pow(2f, -10f * val) + 2f) + start;
	}

	private static float easeInCirc(float start, float end, float val)
	{
		end -= start;
		return (0f - end) * (Mathf.Sqrt(1f - val * val) - 1f) + start;
	}

	private static float easeOutCirc(float start, float end, float val)
	{
		val--;
		end -= start;
		return end * Mathf.Sqrt(1f - val * val) + start;
	}

	private static float easeInOutCirc(float start, float end, float val)
	{
		val /= 0.5f;
		end -= start;
		if (val < 1f)
		{
			return (0f - end) / 2f * (Mathf.Sqrt(1f - val * val) - 1f) + start;
		}
		val -= 2f;
		return end / 2f * (Mathf.Sqrt(1f - val * val) + 1f) + start;
	}

	private static float easeInBounce(float start, float end, float val)
	{
		end -= start;
		float num = 1f;
		return end - easeOutBounce(0f, end, num - val) + start;
	}

	private static float easeOutBounce(float start, float end, float val)
	{
		val /= 1f;
		end -= start;
		if (val < 372f / 1023f)
		{
			return end * (7.5625f * val * val) + start;
		}
		if (val < 744f / 1023f)
		{
			val -= 558f / 1023f;
			return end * (7.5625f * val * val + 0.75f) + start;
		}
		if ((double)val < 930.0 / 1023.0)
		{
			val -= 837f / 1023f;
			return end * (7.5625f * val * val + 0.9375f) + start;
		}
		val -= 21f / 22f;
		return end * (7.5625f * val * val + 63f / 64f) + start;
	}

	private static float easeInOutBounce(float start, float end, float val)
	{
		end -= start;
		float num = 1f;
		if (val < num / 2f)
		{
			return easeInBounce(0f, end, val * 2f) * 0.5f + start;
		}
		return easeOutBounce(0f, end, val * 2f - num) * 0.5f + end * 0.5f + start;
	}

	private static float easeInBack(float start, float end, float val, float overshoot = 1f)
	{
		end -= start;
		val /= 1f;
		float num = 1.70158f * overshoot;
		return end * val * val * ((num + 1f) * val - num) + start;
	}

	private static float easeOutBack(float start, float end, float val, float overshoot = 1f)
	{
		float num = 1.70158f * overshoot;
		end -= start;
		val = val / 1f - 1f;
		return end * (val * val * ((num + 1f) * val + num) + 1f) + start;
	}

	private static float easeInOutBack(float start, float end, float val, float overshoot = 1f)
	{
		float num = 1.70158f * overshoot;
		end -= start;
		val /= 0.5f;
		if (val < 1f)
		{
			num *= 1.525f * overshoot;
			return end / 2f * (val * val * ((num + 1f) * val - num)) + start;
		}
		val -= 2f;
		num *= 1.525f * overshoot;
		return end / 2f * (val * val * ((num + 1f) * val + num) + 2f) + start;
	}

	private static float easeInElastic(float start, float end, float val, float overshoot = 1f, float period = 0.3f)
	{
		end -= start;
		float num = 0f;
		float num2 = 0f;
		if (val == 0f)
		{
			return start;
		}
		if (val == 1f)
		{
			return start + end;
		}
		if (num2 == 0f || num2 < Mathf.Abs(end))
		{
			num2 = end;
			num = period / 4f;
		}
		else
		{
			num = period / ((float)Math.PI * 2f) * Mathf.Asin(end / num2);
		}
		if (overshoot > 1f && val > 0.6f)
		{
			overshoot = 1f + (1f - val) / 0.4f * (overshoot - 1f);
		}
		val--;
		return start - num2 * Mathf.Pow(2f, 10f * val) * Mathf.Sin((val - num) * ((float)Math.PI * 2f) / period) * overshoot;
	}

	private static float easeOutElastic(float start, float end, float val, float overshoot = 1f, float period = 0.3f)
	{
		end -= start;
		float num = 0f;
		float num2 = 0f;
		if (val == 0f)
		{
			return start;
		}
		if (val == 1f)
		{
			return start + end;
		}
		if (num2 == 0f || num2 < Mathf.Abs(end))
		{
			num2 = end;
			num = period / 4f;
		}
		else
		{
			num = period / ((float)Math.PI * 2f) * Mathf.Asin(end / num2);
		}
		if (overshoot > 1f && val < 0.4f)
		{
			overshoot = 1f + val / 0.4f * (overshoot - 1f);
		}
		return start + end + num2 * Mathf.Pow(2f, -10f * val) * Mathf.Sin((val - num) * ((float)Math.PI * 2f) / period) * overshoot;
	}

	private static float easeInOutElastic(float start, float end, float val, float overshoot = 1f, float period = 0.3f)
	{
		end -= start;
		float num = 0f;
		float num2 = 0f;
		if (val == 0f)
		{
			return start;
		}
		val /= 0.5f;
		if (val == 2f)
		{
			return start + end;
		}
		if (num2 == 0f || num2 < Mathf.Abs(end))
		{
			num2 = end;
			num = period / 4f;
		}
		else
		{
			num = period / ((float)Math.PI * 2f) * Mathf.Asin(end / num2);
		}
		if (overshoot > 1f)
		{
			if (val < 0.2f)
			{
				overshoot = 1f + val / 0.2f * (overshoot - 1f);
			}
			else if (val > 0.8f)
			{
				overshoot = 1f + (1f - val) / 0.2f * (overshoot - 1f);
			}
		}
		if (val < 1f)
		{
			val--;
			return start - 0.5f * (num2 * Mathf.Pow(2f, 10f * val) * Mathf.Sin((val - num) * ((float)Math.PI * 2f) / period)) * overshoot;
		}
		val--;
		return end + start + num2 * Mathf.Pow(2f, -10f * val) * Mathf.Sin((val - num) * ((float)Math.PI * 2f) / period) * 0.5f * overshoot;
	}

	public static void addListener(int eventId, Action<LTEvent> callback)
	{
		addListener(tweenEmpty, eventId, callback);
	}

	public static void addListener(GameObject caller, int eventId, Action<LTEvent> callback)
	{
		if (eventListeners == null)
		{
			INIT_LISTENERS_MAX = LISTENERS_MAX;
			eventListeners = new Action<LTEvent>[EVENTS_MAX * LISTENERS_MAX];
			goListeners = new GameObject[EVENTS_MAX * LISTENERS_MAX];
		}
		for (i = 0; i < INIT_LISTENERS_MAX; i++)
		{
			int num = eventId * INIT_LISTENERS_MAX + i;
			if ((Object)(object)goListeners[num] == (Object)null || eventListeners[num] == null)
			{
				eventListeners[num] = callback;
				goListeners[num] = caller;
				if (i >= eventsMaxSearch)
				{
					eventsMaxSearch = i + 1;
				}
				return;
			}
			if ((Object)(object)goListeners[num] == (Object)(object)caller && object.Equals(eventListeners[num], callback))
			{
				return;
			}
		}
		Debug.LogError((object)("You ran out of areas to add listeners, consider increasing LISTENERS_MAX, ex: LeanTween.LISTENERS_MAX = " + LISTENERS_MAX * 2));
	}

	public static bool removeListener(int eventId, Action<LTEvent> callback)
	{
		return removeListener(tweenEmpty, eventId, callback);
	}

	public static bool removeListener(GameObject caller, int eventId, Action<LTEvent> callback)
	{
		for (i = 0; i < eventsMaxSearch; i++)
		{
			int num = eventId * INIT_LISTENERS_MAX + i;
			if ((Object)(object)goListeners[num] == (Object)(object)caller && object.Equals(eventListeners[num], callback))
			{
				eventListeners[num] = null;
				goListeners[num] = null;
				return true;
			}
		}
		return false;
	}

	public static void dispatchEvent(int eventId)
	{
		dispatchEvent(eventId, null);
	}

	public static void dispatchEvent(int eventId, object data)
	{
		for (int i = 0; i < eventsMaxSearch; i++)
		{
			int num = eventId * INIT_LISTENERS_MAX + i;
			if (eventListeners[num] != null)
			{
				if (Object.op_Implicit((Object)(object)goListeners[num]))
				{
					eventListeners[num](new LTEvent(eventId, data));
				}
				else
				{
					eventListeners[num] = null;
				}
			}
		}
	}

	static LeanTween()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Expected Obj, but got Unknown
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Expected Obj, but got Unknown
	}
}
