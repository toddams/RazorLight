using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace RazorLight.Precompile
{
	public class JsonModel : DynamicObject
	{
		private readonly Dictionary<string, object> m_properties = new();

		public static object New(JToken jsonToken)
		{
			return jsonToken switch
			{
				JObject o => new JsonModel(o),
				JArray a => a.Select(New).ToList(),
				JValue v => v.Value,
				_ => jsonToken,
			};
		}

		public JsonModel(JObject o)
		{
			foreach (var (key, value) in o)
			{
				m_properties[key] = New(value);
			}
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			if (!m_properties.TryGetValue(binder.Name, out result))
			{
				result = null;
			}
			return true;
		}
	}
}
