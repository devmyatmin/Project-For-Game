using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby;

public sealed class LobbyDetails : Handle
{
	public LobbyDetails(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public ProductUserId GetLobbyOwner(LobbyDetailsGetLobbyOwnerOptions options)
	{
		LobbyDetailsGetLobbyOwnerOptionsInternal options2 = Helper.CopyProperties<LobbyDetailsGetLobbyOwnerOptionsInternal>(options);
		IntPtr source = EOS_LobbyDetails_GetLobbyOwner(InnerHandle, ref options2);
		Helper.TryMarshalDispose(ref options2);
		ProductUserId target = Helper.GetDefault<ProductUserId>();
		Helper.TryMarshalGet(source, out target);
		return target;
	}

	public Result CopyInfo(LobbyDetailsCopyInfoOptions options, out LobbyDetailsInfo outLobbyDetailsInfo)
	{
		LobbyDetailsCopyInfoOptionsInternal options2 = Helper.CopyProperties<LobbyDetailsCopyInfoOptionsInternal>(options);
		outLobbyDetailsInfo = Helper.GetDefault<LobbyDetailsInfo>();
		IntPtr outLobbyDetailsInfo2 = IntPtr.Zero;
		Result source = EOS_LobbyDetails_CopyInfo(InnerHandle, ref options2, ref outLobbyDetailsInfo2);
		Helper.TryMarshalDispose(ref options2);
		if (Helper.TryMarshalGet<LobbyDetailsInfoInternal, LobbyDetailsInfo>(outLobbyDetailsInfo2, out outLobbyDetailsInfo))
		{
			EOS_LobbyDetails_Info_Release(outLobbyDetailsInfo2);
		}
		Result target = Helper.GetDefault<Result>();
		Helper.TryMarshalGet(source, out target);
		return target;
	}

	public uint GetAttributeCount(LobbyDetailsGetAttributeCountOptions options)
	{
		LobbyDetailsGetAttributeCountOptionsInternal options2 = Helper.CopyProperties<LobbyDetailsGetAttributeCountOptionsInternal>(options);
		uint source = EOS_LobbyDetails_GetAttributeCount(InnerHandle, ref options2);
		Helper.TryMarshalDispose(ref options2);
		uint target = Helper.GetDefault<uint>();
		Helper.TryMarshalGet(source, out target);
		return target;
	}

	public Result CopyAttributeByIndex(LobbyDetailsCopyAttributeByIndexOptions options, out Attribute outAttribute)
	{
		LobbyDetailsCopyAttributeByIndexOptionsInternal options2 = Helper.CopyProperties<LobbyDetailsCopyAttributeByIndexOptionsInternal>(options);
		outAttribute = Helper.GetDefault<Attribute>();
		IntPtr outAttribute2 = IntPtr.Zero;
		Result source = EOS_LobbyDetails_CopyAttributeByIndex(InnerHandle, ref options2, ref outAttribute2);
		Helper.TryMarshalDispose(ref options2);
		if (Helper.TryMarshalGet<AttributeInternal, Attribute>(outAttribute2, out outAttribute))
		{
			EOS_Lobby_Attribute_Release(outAttribute2);
		}
		Result target = Helper.GetDefault<Result>();
		Helper.TryMarshalGet(source, out target);
		return target;
	}

	public Result CopyAttributeByKey(LobbyDetailsCopyAttributeByKeyOptions options, out Attribute outAttribute)
	{
		LobbyDetailsCopyAttributeByKeyOptionsInternal options2 = Helper.CopyProperties<LobbyDetailsCopyAttributeByKeyOptionsInternal>(options);
		outAttribute = Helper.GetDefault<Attribute>();
		IntPtr outAttribute2 = IntPtr.Zero;
		Result source = EOS_LobbyDetails_CopyAttributeByKey(InnerHandle, ref options2, ref outAttribute2);
		Helper.TryMarshalDispose(ref options2);
		if (Helper.TryMarshalGet<AttributeInternal, Attribute>(outAttribute2, out outAttribute))
		{
			EOS_Lobby_Attribute_Release(outAttribute2);
		}
		Result target = Helper.GetDefault<Result>();
		Helper.TryMarshalGet(source, out target);
		return target;
	}

	public uint GetMemberCount(LobbyDetailsGetMemberCountOptions options)
	{
		LobbyDetailsGetMemberCountOptionsInternal options2 = Helper.CopyProperties<LobbyDetailsGetMemberCountOptionsInternal>(options);
		uint source = EOS_LobbyDetails_GetMemberCount(InnerHandle, ref options2);
		Helper.TryMarshalDispose(ref options2);
		uint target = Helper.GetDefault<uint>();
		Helper.TryMarshalGet(source, out target);
		return target;
	}

	public ProductUserId GetMemberByIndex(LobbyDetailsGetMemberByIndexOptions options)
	{
		LobbyDetailsGetMemberByIndexOptionsInternal options2 = Helper.CopyProperties<LobbyDetailsGetMemberByIndexOptionsInternal>(options);
		IntPtr source = EOS_LobbyDetails_GetMemberByIndex(InnerHandle, ref options2);
		Helper.TryMarshalDispose(ref options2);
		ProductUserId target = Helper.GetDefault<ProductUserId>();
		Helper.TryMarshalGet(source, out target);
		return target;
	}

	public uint GetMemberAttributeCount(LobbyDetailsGetMemberAttributeCountOptions options)
	{
		LobbyDetailsGetMemberAttributeCountOptionsInternal options2 = Helper.CopyProperties<LobbyDetailsGetMemberAttributeCountOptionsInternal>(options);
		uint source = EOS_LobbyDetails_GetMemberAttributeCount(InnerHandle, ref options2);
		Helper.TryMarshalDispose(ref options2);
		uint target = Helper.GetDefault<uint>();
		Helper.TryMarshalGet(source, out target);
		return target;
	}

	public Result CopyMemberAttributeByIndex(LobbyDetailsCopyMemberAttributeByIndexOptions options, out Attribute outAttribute)
	{
		LobbyDetailsCopyMemberAttributeByIndexOptionsInternal options2 = Helper.CopyProperties<LobbyDetailsCopyMemberAttributeByIndexOptionsInternal>(options);
		outAttribute = Helper.GetDefault<Attribute>();
		IntPtr outAttribute2 = IntPtr.Zero;
		Result source = EOS_LobbyDetails_CopyMemberAttributeByIndex(InnerHandle, ref options2, ref outAttribute2);
		Helper.TryMarshalDispose(ref options2);
		if (Helper.TryMarshalGet<AttributeInternal, Attribute>(outAttribute2, out outAttribute))
		{
			EOS_Lobby_Attribute_Release(outAttribute2);
		}
		Result target = Helper.GetDefault<Result>();
		Helper.TryMarshalGet(source, out target);
		return target;
	}

	public Result CopyMemberAttributeByKey(LobbyDetailsCopyMemberAttributeByKeyOptions options, out Attribute outAttribute)
	{
		LobbyDetailsCopyMemberAttributeByKeyOptionsInternal options2 = Helper.CopyProperties<LobbyDetailsCopyMemberAttributeByKeyOptionsInternal>(options);
		outAttribute = Helper.GetDefault<Attribute>();
		IntPtr outAttribute2 = IntPtr.Zero;
		Result source = EOS_LobbyDetails_CopyMemberAttributeByKey(InnerHandle, ref options2, ref outAttribute2);
		Helper.TryMarshalDispose(ref options2);
		if (Helper.TryMarshalGet<AttributeInternal, Attribute>(outAttribute2, out outAttribute))
		{
			EOS_Lobby_Attribute_Release(outAttribute2);
		}
		Result target = Helper.GetDefault<Result>();
		Helper.TryMarshalGet(source, out target);
		return target;
	}

	public void Release()
	{
		EOS_LobbyDetails_Release(InnerHandle);
	}

	[DllImport("EOSSDK-Win64-Shipping")]
	private static extern void EOS_Lobby_Attribute_Release(IntPtr lobbyAttribute);

	[DllImport("EOSSDK-Win64-Shipping")]
	private static extern void EOS_LobbyDetails_Info_Release(IntPtr lobbyDetailsInfo);

	[DllImport("EOSSDK-Win64-Shipping")]
	private static extern void EOS_LobbyDetails_Release(IntPtr lobbyHandle);

	[DllImport("EOSSDK-Win64-Shipping")]
	private static extern Result EOS_LobbyDetails_CopyMemberAttributeByKey(IntPtr handle, ref LobbyDetailsCopyMemberAttributeByKeyOptionsInternal options, ref IntPtr outAttribute);

	[DllImport("EOSSDK-Win64-Shipping")]
	private static extern Result EOS_LobbyDetails_CopyMemberAttributeByIndex(IntPtr handle, ref LobbyDetailsCopyMemberAttributeByIndexOptionsInternal options, ref IntPtr outAttribute);

	[DllImport("EOSSDK-Win64-Shipping")]
	private static extern uint EOS_LobbyDetails_GetMemberAttributeCount(IntPtr handle, ref LobbyDetailsGetMemberAttributeCountOptionsInternal options);

	[DllImport("EOSSDK-Win64-Shipping")]
	private static extern IntPtr EOS_LobbyDetails_GetMemberByIndex(IntPtr handle, ref LobbyDetailsGetMemberByIndexOptionsInternal options);

	[DllImport("EOSSDK-Win64-Shipping")]
	private static extern uint EOS_LobbyDetails_GetMemberCount(IntPtr handle, ref LobbyDetailsGetMemberCountOptionsInternal options);

	[DllImport("EOSSDK-Win64-Shipping")]
	private static extern Result EOS_LobbyDetails_CopyAttributeByKey(IntPtr handle, ref LobbyDetailsCopyAttributeByKeyOptionsInternal options, ref IntPtr outAttribute);

	[DllImport("EOSSDK-Win64-Shipping")]
	private static extern Result EOS_LobbyDetails_CopyAttributeByIndex(IntPtr handle, ref LobbyDetailsCopyAttributeByIndexOptionsInternal options, ref IntPtr outAttribute);

	[DllImport("EOSSDK-Win64-Shipping")]
	private static extern uint EOS_LobbyDetails_GetAttributeCount(IntPtr handle, ref LobbyDetailsGetAttributeCountOptionsInternal options);

	[DllImport("EOSSDK-Win64-Shipping")]
	private static extern Result EOS_LobbyDetails_CopyInfo(IntPtr handle, ref LobbyDetailsCopyInfoOptionsInternal options, ref IntPtr outLobbyDetailsInfo);

	[DllImport("EOSSDK-Win64-Shipping")]
	private static extern IntPtr EOS_LobbyDetails_GetLobbyOwner(IntPtr handle, ref LobbyDetailsGetLobbyOwnerOptionsInternal options);
}
