using System;
using System.Globalization;
using Beam.Serialization.Json;

[CustomSerializerFor(typeof(DateTime))]
public class DateTimeSerializer : Serializer<DateTime>
{
	public override JObject Save(DateTime instance)
	{
		return new JObject((JObject dateTime) =>
		{
			dateTime.AddField("Year", instance.Year);
			dateTime.AddField("Month", instance.Month);
			dateTime.AddField("Day", instance.Day);
			dateTime.AddField("Hour", instance.Hour);
			dateTime.AddField("Minute", instance.Minute);
			dateTime.AddField("Second", instance.Second);
		});
	}

	public override DateTime Load(JObject data)
	{
		return new DateTime(data["Year"].GetValue<int>(), data["Month"].GetValue<int>(), data["Day"].GetValue<int>(), data["Hour"].GetValue<int>(), data["Minute"].GetValue<int>(), data["Second"].GetValue<int>(), 0, CultureInfo.InvariantCulture.Calendar);
	}
}
