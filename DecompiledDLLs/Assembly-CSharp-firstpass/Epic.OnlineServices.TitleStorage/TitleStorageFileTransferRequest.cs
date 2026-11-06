using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Epic.OnlineServices.TitleStorage;

public sealed class TitleStorageFileTransferRequest : Handle
{
	public TitleStorageFileTransferRequest(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public Result GetFileRequestState()
	{
		Result source = EOS_TitleStorageFileTransferRequest_GetFileRequestState(InnerHandle);
		Result target = Helper.GetDefault<Result>();
		Helper.TryMarshalGet(source, out target);
		return target;
	}

	public Result GetFilename(uint filenameStringBufferSizeBytes, StringBuilder outStringBuffer, out int outStringLength)
	{
		outStringLength = Helper.GetDefault<int>();
		Result source = EOS_TitleStorageFileTransferRequest_GetFilename(InnerHandle, filenameStringBufferSizeBytes, outStringBuffer, ref outStringLength);
		Result target = Helper.GetDefault<Result>();
		Helper.TryMarshalGet(source, out target);
		return target;
	}

	public Result CancelRequest()
	{
		Result source = EOS_TitleStorageFileTransferRequest_CancelRequest(InnerHandle);
		Result target = Helper.GetDefault<Result>();
		Helper.TryMarshalGet(source, out target);
		return target;
	}

	public void Release()
	{
		EOS_TitleStorageFileTransferRequest_Release(InnerHandle);
	}

	[DllImport("EOSSDK-Win64-Shipping")]
	private static extern void EOS_TitleStorageFileTransferRequest_Release(IntPtr titleStorageFileTransferHandle);

	[DllImport("EOSSDK-Win64-Shipping")]
	private static extern Result EOS_TitleStorageFileTransferRequest_CancelRequest(IntPtr handle);

	[DllImport("EOSSDK-Win64-Shipping")]
	private static extern Result EOS_TitleStorageFileTransferRequest_GetFilename(IntPtr handle, uint filenameStringBufferSizeBytes, StringBuilder outStringBuffer, ref int outStringLength);

	[DllImport("EOSSDK-Win64-Shipping")]
	private static extern Result EOS_TitleStorageFileTransferRequest_GetFileRequestState(IntPtr handle);
}
