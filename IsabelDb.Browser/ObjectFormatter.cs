using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace IsabelDb.Browser
{
	public sealed class ObjectFormatter
	{
		private readonly HashSet<Type> _nativeTypes;
		private readonly Dictionary<Type, Func<object, ObjectModel>> _builtInTypes;

		public ObjectFormatter()
		{
			_nativeTypes = new HashSet<Type>
			{
				typeof(string),
				typeof(Int32),
				typeof(Int64),
				typeof(float),
				typeof(double)
			};

			_builtInTypes = new Dictionary<Type, Func<object, ObjectModel>>
			{
				{typeof(KeyValuePair<,>), CreateKeyValuePairModel},
				{typeof(IEnumerable<>), CreateEnumerationModel}
			};
		}

		public string Preview(object value, int maximumLength = int.MaxValue)
		{
			return Preview(CultureInfo.CurrentUICulture, value, maximumLength);
		}

		public string Preview(IFormatProvider formatProvider, object value, int maximumLength = Int32.MaxValue)
		{
			var model = CreateModel(value);
			var builder = new JsonBuilder(maximumLength);
			builder.FormatPreview(model);
			return builder.ToString();
		}

		ObjectModel CreateModel(object value)
		{
			var type = value.GetType();
			if (_nativeTypes.Contains(type))
			{
				return new ObjectModel
				{
					Value = value
				};
			}

			if (TryCreateModel(value, type, out var objectModel))
				return objectModel;

			var interfaces = type.GetInterfaces();
			foreach(var @interface in interfaces)
				if (TryCreateModel(value, @interface, out objectModel))
					return objectModel;

			return CreateModel(value,
			                         type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
			                             .Where(x => x.GetCustomAttribute<DataMemberAttribute>() != null),
			                         type.GetFields(BindingFlags.Public | BindingFlags.Instance)
			                             .Where(x => x.GetCustomAttribute<DataMemberAttribute>() != null));
		}

		private bool TryCreateModel(object value, Type type, out ObjectModel objectModel)
		{
			var tmp = type;
			while (tmp != null)
			{
				if (_builtInTypes.TryGetValue(tmp, out var fn))
				{
					objectModel = fn(value);
					return true;
				}

				if (tmp.IsGenericType)
				{
					var definition = tmp.GetGenericTypeDefinition();
					if (definition != null && _builtInTypes.TryGetValue(definition, out fn))
					{
						objectModel = fn(value);
						return true;
					}
				}

				tmp = tmp.BaseType;
			}

			objectModel = new ObjectModel();
			return false;
		}

		private ObjectModel CreateModel(object value,
		                           IEnumerable<PropertyInfo> properties,
		                           IEnumerable<FieldInfo> fields)
		{
			var ret = new List<Property>();
			foreach (var property in properties)
			{
				ret.Add(new Property
				{
					Name = property.Name,
					Value = CreateModel(property.GetValue(value))
				});
			}
			foreach (var field in fields)
			{
				ret.Add(new Property
				{
					Name = field.Name,
					Value = CreateModel(field.GetValue(value))
				});
			}

			return new ObjectModel
			{
				Properties = ret
			};
		}

		private ObjectModel CreateKeyValuePairModel(object value)
		{
			var type = value.GetType();
			var keyProperty = type.GetProperty("Key");
			var valueProperty = type.GetProperty("Value");

			return new ObjectModel
			{
				Properties = new[]
				{
					new Property {Name = "Key", Value = CreateModel(keyProperty.GetValue(value))},
					new Property {Name = "Value", Value = CreateModel(valueProperty.GetValue(value))}
				}
			};
		}

		private ObjectModel CreateEnumerationModel(object value)
		{
			var enumerator = ((IEnumerable) value).GetEnumerator();
			var values = new List<ObjectModel>();
			while (enumerator.MoveNext())
			{
				values.Add(CreateModel(enumerator.Current));
			}

			return new ObjectModel
			{
				Values = values
			};
		}

		sealed class JsonBuilder
		{
			private readonly int _maximumLength;
			private readonly StringBuilder _builder;
			private bool _appendedEllipses;

			private const int EllipsisLength = 3;

			public JsonBuilder(int maximumLength)
			{
				_maximumLength = maximumLength;
				_builder = new StringBuilder();
			}

			public void FormatPreview(ObjectModel model)
			{
				if (model.Value != null)
				{
					FormatValue(model.Value);
				}
				else if (model.Values != null)
				{
					_builder.Append("{");
					for (int i = 0; i < model.Values.Count; ++i)
					{
						var property = model.Values[i];
						if (i > 0)
							_builder.Append(", ");
						FormatPreview(property);

						if (_appendedEllipses)
							return;
					}
					_builder.Append("}");
				}
				else
				{
					_builder.Append("{");
					for (int i = 0; i < model.Properties.Count; ++i)
					{
						var property = model.Properties[i];
						if (i > 0)
							_builder.Append(", ");
						_builder.AppendFormat("{0}: ", property.Name);
						FormatPreview(property.Value);

						if (_appendedEllipses)
							return;
					}
					_builder.Append("}");
				}

				AppendEllipsesIfNecessary();
			}

			private void FormatValue(object value)
			{
				if (value is string)
				{
					_builder.AppendFormat("\"{0}\"", value);
				}
				else
				{
					_builder.Append(value);
				}

				AppendEllipsesIfNecessary();
			}

			private void AppendEllipsesIfNecessary()
			{
				if (ResultIsTooLong && !_appendedEllipses)
					AppendEllipses();
			}

			private void AppendEllipses()
			{
				var overflow = Overflow;
				if (Overflow > 0)
					_builder.Remove(_builder.Length - overflow, overflow);
				_builder.Append('.', EllipsisLength);
				_appendedEllipses = true;
			}

			private int Overflow => _builder.Length - _maximumLength + EllipsisLength;

			private bool ResultIsTooLong => Overflow > 0;

			#region Overrides of Object

			public override string ToString()
			{
				return _builder.ToString();
			}

			#endregion
		}

		struct ObjectModel
		{
			public object Value;
			public IReadOnlyList<Property> Properties;
			public IReadOnlyList<ObjectModel> Values;
		}

		struct Property
		{
			public string Name;
			public ObjectModel Value;
		}
	}
}
