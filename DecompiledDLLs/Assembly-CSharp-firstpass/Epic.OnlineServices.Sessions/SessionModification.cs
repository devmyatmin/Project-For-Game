using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

public sealed class SessionModification : Handle
{
	public SessionModification(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public Result SetBucketId(SessionModificationSetBucketIdOptions options)
	{
		SessionModificationSetBucketIdOptionsInternal options2 = Helper.CopyProperties<SessionModificationSetBucketIdOptionsInternal>(options);
		Result source = EOS_SessionModification_SetBucketId(InnerHandle, ref options2);
		Helper.TryMarshalDispose(ref options2);
		Result target = Helper.GetDefault<Result>();
		Helper.TryMarshalGet(source, out target);
		return target;
	}

	public Result SetHostAddress(SessionModificationSetHostAddressOptions options)
	{
		SessionModificationSetHostAddressOptionsInternal options2 = Helper.CopyProperties<SessionModificationSetHostAddressOptionsInternal>(options);
		Result source = EOS_SessionModification_SetHostAddress(InnerHandle, ref options2);
		Helper.TryMarshalDispose(ref options2);
		Result target = Helper.GetDefault<Result>();
		Helper.TryMarshalGet(source, out target);
		return target;
	}

	public Result SetPermissionLevel(SessionModificationSetPermissionLevelOptions options)
	{
		SessionModificationSetPermissionLevelOptionsInternal options2 = Helper.CopyProperties<SessionModificationSetPermissionLevelOptionsInternal>(options);
		Result source = EOS_SessionModification_SetPermissionLevel(InnerHandle, ref options2);
		Helper.TryMarshalDispose(ref options2);
		Result target = Helper.GetDefault<Result>();
		Helper.TryMarshalGet(source, out target);
		return target;
	}

	public Result SetJoinInProgressAllowed(SessionModificationSetJoinInProgressAllowedOptions options)
	{
		SessionModificationSetJoinInProgressAllowedOptionsInternal options2 = Helper.CopyProperties<SessionModificationSetJoinInProgressAllowedOptionsInternal>(options);
		Result source = EOS_SessionModification_SetJoinInProgressAllowed(InnerHandle, ref options2);
		Helper.TryMarshalDispose(ref options2);
		Result target = Helper.GetDefault<Result>();
		Helper.TryMarshalGet(source, out target);
		return target;
	}

	public Result SetMaxPlayers(SessionModificationSetMaxPlayersOptions options)
	{
		SessionModificationSetMaxPlayersOptionsInternal options2 = Helper.CopyProperties<SessionModificationSetMaxPlayersOptionsInternal>(options);
		Result source = EOS_SessionModification_SetMaxPlayers(InnerHandle, ref options2);
		Helper.TryMarshalDispose(ref options2);
		Result target = Helper.GetDefault<Result>();
		Helper.TryMarshalGet(source, out target);
		return target;
	}

	public Result SetInvitesAllowed(SessionModificationSetInvitesAllowedOptions options)
	{
		SessionModificationSetInvitesAllowedOptionsInternal options2 = Helper.CopyProperties<SessionModificationSetInvitesAllowedOptionsInternal>(options);
		Result source = EOS_SessionModification_SetInvitesAllowed(InnerHandle, ref options2);
		Helper.TryMarshalDispose(ref options2);
		Result target = Helper.GetDefault<Result>();
		Helper.TryMarshalGet(source, out target);
		return target;
	}

	public Result AddAttribute(SessionModificationAddAttributeOptions options)
	{
		SessionModificationAddAttributeOptionsInternal options2 = Helper.CopyProperties<SessionModificationAddAttributeOptionsInternal>(options);
		Result source = EOS_SessionModification_AddAttribute(InnerHandle, ref options2);
		Helper.TryMarshalDispose(ref options2);
		Result target = Helper.GetDefault<Result>();
		Helper.TryMarshalGet(source, out target);
		return target;
	}

	public Result RemoveAttribute(SessionModificationRemoveAttributeOptions options)
	{
		SessionModificationRemoveAttributeOptionsInternal options2 = Helper.CopyProperties<SessionModificationRemoveAttributeOptionsInternal>(options);
		Result source = EOS_SessionModification_RemoveAttribute(InnerHandle, ref options2);
		Helper.TryMarshalDispose(ref options2);
		Result target = Helper.GetDefault<Result>();
		Helper.TryMarshalGet(source, out target);
		return target;
	}

	public void Release()
	{
		EOS_SessionModification_Release(InnerHandle);
	}

	[DllImport("EOSSDK-Win64-Shipping")]
	private static extern void EOS_SessionModification_Release(IntPtr sessionModificationHandle);

	[DllImport("EOSSDK-Win64-Shipping")]
	private static extern Result EOS_SessionModification_RemoveAttribute(IntPtr handle, ref SessionModificationRemoveAttributeOptionsInternal options);

	[DllImport("EOSSDK-Win64-Shipping")]
	private static extern Result EOS_SessionModification_AddAttribute(IntPtr handle, ref SessionModificationAddAttributeOptionsInternal options);

	[DllImport("EOSSDK-Win64-Shipping")]
	private static extern Result EOS_SessionModification_SetInvitesAllowed(IntPtr handle, ref SessionModificationSetInvitesAllowedOptionsInternal options);

	[DllImport("EOSSDK-Win64-Shipping")]
	private static extern Result EOS_SessionModification_SetMaxPlayers(IntPtr handle, ref SessionModificationSetMaxPlayersOptionsInternal options);

	[DllImport("EOSSDK-Win64-Shipping")]
	private static extern Result EOS_SessionModification_SetJoinInProgressAllowed(IntPtr handle, ref SessionModificationSetJoinInProgressAllowedOptionsInternal options);

	[DllImport("EOSSDK-Win64-Shipping")]
	private static extern Result EOS_SessionModification_SetPermissionLevel(IntPtr handle, ref SessionModificationSetPermissionLevelOptionsInternal options);

	[DllImport("EOSSDK-Win64-Shipping")]
	private static extern Result EOS_SessionModification_SetHostAddress(IntPtr handle, ref SessionModificationSetHostAddressOptionsInternal options);

	[DllImport("EOSSDK-Win64-Shipping")]
	private static extern Result EOS_SessionModification_SetBucketId(IntPtr handle, ref SessionModificationSetBucketIdOptionsInternal options);
}
