using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Epic.OnlineServices;

public sealed class EpicAccountId : Handle
{
	public EpicAccountId(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public bool IsValid()
	{
		int source = EOS_EpicAccountId_IsValid(InnerHandle);
		bool target = Helper.GetDefault<bool>();
		Helper.TryMarshalGet(source, out target);
		return target;
	}

	public Result ToString(StringBuilder outBuffer, ref int inOutBufferLength)
	{
		Result source = EOS_EpicAccountId_ToString(InnerHandle, outBuffer, ref inOutBufferLength);
		Result target = Helper.GetDefault<Result>();
		Helper.TryMarshalGet(source, out target);
		return target;
	}

	public static EpicAccountId FromString(string accountIdString)
	{
		IntPtr source = EOS_EpicAccountId_FromString(accountIdString);
		EpicAccountId target = Helper.GetDefault<EpicAccountId>();
		Helper.TryMarshalGet(source, out target);
		return target;
	}

	[DllImport("EOSSDK-Win64-Shipping")]
	private static extern IntPtr EOS_EpicAccountId_FromString([MarshalAs(UnmanagedType.LPStr)] string accountIdString);

	[DllImport("EOSSDK-Win64-Shipping")]
	private static extern Result EOS_EpicAccountId_ToString(IntPtr accountId, StringBuilder outBuffer, ref int inOutBufferLength);

	[DllImport("EOSSDK-Win64-Shipping")]
	private static extern int EOS_EpicAccountId_IsValid(IntPtr accountId);
}
