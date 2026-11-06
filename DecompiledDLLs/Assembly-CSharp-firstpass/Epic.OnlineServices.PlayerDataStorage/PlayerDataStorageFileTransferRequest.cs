using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Epic.OnlineServices.PlayerDataStorage;

public sealed class PlayerDataStorageFileTransferRequest : Handle
{
	public PlayerDataStorageFileTransferRequest(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public Result GetFileRequestState()
	{
		Result source = EOS_PlayerDataStorageFileTransferRequest_GetFileRequestState(InnerHandle);
		Result target = Helper.GetDefault<Result>();
		Helper.TryMarshalGet(source, out target);
		return target;
	}

	public Result GetFilename(uint filenameStringBufferSizeBytes, StringBuilder outStringBuffer, out int outStringLength)
	{
		outStringLength = Helper.GetDefault<int>();
		Result source = EOS_PlayerDataStorageFileTransferRequest_GetFilename(InnerHandle, filenameStringBufferSizeBytes, outStringBuffer, ref outStringLength);
		Result target = Helper.GetDefault<Result>();
		Helper.TryMarshalGet(source, out target);
		return target;
	}

	public Result CancelRequest()
	{
		Result source = EOS_PlayerDataStorageFileTransferRequest_CancelRequest(InnerHandle);
		Result target = Helper.GetDefault<Result>();
		Helper.TryMarshalGet(source, out target);
		return target;
	}

	public void Release()
	{
		EOS_PlayerDataStorageFileTransferRequest_Release(InnerHandle);
	}

	[DllImport("EOSSDK-Win64-Shipping")]
	private static extern void EOS_PlayerDataStorageFileTransferRequest_Release(IntPtr playerDataStorageFileTransferHandle);

	[DllImport("EOSSDK-Win64-Shipping")]
	private static extern Result EOS_PlayerDataStorageFileTransferRequest_CancelRequest(IntPtr handle);

	[DllImport("EOSSDK-Win64-Shipping")]
	private static extern Result EOS_PlayerDataStorageFileTransferRequest_GetFilename(IntPtr handle, uint filenameStringBufferSizeBytes, StringBuilder outStringBuffer, ref int outStringLength);

	[DllImport("EOSSDK-Win64-Shipping")]
	private static extern Result EOS_PlayerDataStorageFileTransferRequest_GetFileRequestState(IntPtr handle);
}
