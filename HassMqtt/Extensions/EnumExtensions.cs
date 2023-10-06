using System.Reflection;
using System.Runtime.Serialization;

namespace HassMqtt.Extensions {
    public static class EnumExtensions {
        /// <summary>
        /// Returns the EnumMember value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ? GetEnumMemberValue<T>( this T value ) where T : Enum {
            return typeof( T )
                .GetTypeInfo()
                .DeclaredMembers
                .SingleOrDefault( x => x.Name == value.ToString() )
                ?.GetCustomAttribute<EnumMemberAttribute>( false )
                ?.Value;
        }
/*
        /// <summary>
        /// Gets the description of the provided enum
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static string ? GetDescription( this Enum value ) {
            var fieldInfo = value.GetType().GetField(value.ToString());

            if( fieldInfo == null ) {
                return null;
            }

            var attribute = fieldInfo.GetCustomAttribute( typeof( DescriptionAttribute )) as DescriptionAttribute;

            return attribute?.Description ?? "?";
        }

        public static ( int key, string description ) GetLocalizedDescriptionAndKey( this Enum enumValue ) {
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

            var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

            var description = attributes.Length > 0 ? attributes[0].Description : enumValue.ToString();

            var enumIndex = Array.IndexOf(Enum.GetValues(enumValue.GetType()), enumValue);
            var key = (int)Enum.GetValues(enumValue.GetType()).GetValue(enumIndex);

            return( key, description );
        }
*/
    }
}
