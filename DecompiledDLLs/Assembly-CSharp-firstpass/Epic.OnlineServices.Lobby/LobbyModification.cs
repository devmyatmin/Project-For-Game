using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby;

public sealed class LobbyModification : Handle
{
	public LobbyModification(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public Result SetPermissionLevel(LobbyModificationSetPermissionLevelOptions options)
	{
		LobbyModificationSetPermissionLevelOptionsInternal options2 = Helper.CopyProperties<LobbyModificationSetPermissionLevelOptionsInternal>(options);
		Result source = EOS_LobbyModification_SetPermissionLevel(InnerHandle, ref options2);
		Helper.TryMarshalDispose(ref options2);
		Result target = Helper.GetDefault<Result>();
		Helper.TryMarshalGet(source, out target);
		return target;
	}

	public Result SetMaxMembers(LobbyModificationSetMaxMembersOptions options)
	{
		LobbyModificationSetMaxMembersOptionsInternal options2 = Helper.CopyProperties<LobbyModificationSetMaxMembersOptionsInternal>(options);
		Result source = EOS_LobbyModification_SetMaxMembers(InnerHandle, ref options2);
		Helper.TryMarshalDispose(ref options2);
		Result target = Helper.GetDefault<Result>();
		Helper.TryMarshalGet(source, out target);
		return target;
	}

	public Result AddAttribute(LobbyModificationAddAttributeOptions options)
	{
		LobbyModificationAddAttributeOptionsInternal options2 = Helper.CopyProperties<LobbyModificationAddAttributeOptionsInternal>(options);
		Result source = EOS_LobbyModification_AddAttribute(InnerHandle, ref options2);
		Helper.TryMarshalDispose(ref options2);
		Result target = Helper.GetDefault<Result>();
		Helper.TryMarshalGet(source, out target);
		return target;
	}

	public Result RemoveAttribute(LobbyModificationRemoveAttributeOptions options)
	{
		LobbyModificationRemoveAttributeOptionsInternal options2 = Helper.CopyProperties<LobbyModificationRemoveAttributeOptionsInternal>(options);
		Result source = EOS_LobbyModification_RemoveAttribute(InnerHandle, ref options2);
		Helper.TryMarshalDispose(ref options2);
		Result target = Helper.GetDefault<Result>();
		Helper.TryMarshalGet(source, out target);
		return target;
	}

	public Result AddMemberAttribute(LobbyModificationAddMemberAttributeOptions options)
	{
		LobbyModificationAddMemberAttributeOptionsInternal options2 = Helper.CopyProperties<LobbyModificationAddMemberAttributeOptionsInternal>(options);
		Result source = EOS_LobbyModification_AddMemberAttribute(InnerHandle, ref options2);
		Helper.TryMarshalDispose(ref options2);
		Result target = Helper.GetDefault<Result>();
		Helper.TryMarshalGet(source, out target);
		return target;
	}

	public Result RemoveMemberAttribute(LobbyModificationRemoveMemberAttributeOptions options)
	{
		LobbyModificationRemoveMemberAttributeOptionsInternal options2 = Helper.CopyProperties<LobbyModificationRemoveMemberAttributeOptionsInternal>(options);
		Result source = EOS_LobbyModification_RemoveMemberAttribute(InnerHandle, ref options2);
		Helper.TryMarshalDispose(ref options2);
		Result target = Helper.GetDefault<Result>();
		Helper.TryMarshalGet(source, out target);
		return target;
	}

	public void Release()
	{
		EOS_LobbyModification_Release(InnerHandle);
	}

	[DllImport("EOSSDK-Win64-Shipping")]
	private static extern void EOS_LobbyModification_Release(IntPtr lobbyModificationHandle);

	[DllImport("EOSSDK-Win64-Shipping")]
	private static extern Result EOS_LobbyModification_RemoveMemberAttribute(IntPtr handle, ref LobbyModificationRemoveMemberAttributeOptionsInternal options);

	[DllImport("EOSSDK-Win64-Shipping")]
	private static extern Result EOS_LobbyModification_AddMemberAttribute(IntPtr handle, ref LobbyModificationAddMemberAttributeOptionsInternal options);

	[DllImport("EOSSDK-Win64-Shipping")]
	private static extern Result EOS_LobbyModification_RemoveAttribute(IntPtr handle, ref LobbyModificationRemoveAttributeOptionsInternal options);

	[DllImport("EOSSDK-Win64-Shipping")]
	private static extern Result EOS_LobbyModification_AddAttribute(IntPtr handle, ref LobbyModificationAddAttributeOptionsInternal options);

	[DllImport("EOSSDK-Win64-Shipping")]
	private static extern Result EOS_LobbyModification_SetMaxMembers(IntPtr handle, ref LobbyModificationSetMaxMembersOptionsInternal options);

	[DllImport("EOSSDK-Win64-Shipping")]
	private static extern Result EOS_LobbyModification_SetPermissionLevel(IntPtr handle, ref LobbyModificationSetPermissionLevelOptionsInternal options);
}
