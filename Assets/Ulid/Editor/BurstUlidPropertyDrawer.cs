using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace LibSugar.Unity.Editor
{

    [StructLayout(LayoutKind.Explicit, Size = 16)]
    struct ULong2
    {
        [FieldOffset(0)]
        internal ulong _0;
        [FieldOffset(sizeof(ulong))]
        internal ulong _1;
    }

    [CustomPropertyDrawer(typeof(BurstUlid))]
    public class BurstUlidPropertyDrawer : PropertyDrawer
    {
        public BurstUlidPropertyDrawer()
        {
            BurstUlid.InitStatic();
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var textRect = new Rect(position.x, position.y, position.width - 52, position.height);
            var btnRect = new Rect(position.x + position.width - 50, position.y, 50, position.height);

            ULong2 ulong2;
            ulong2._0 = property.FindPropertyRelative("_0").ulongValue;
            ulong2._1 = property.FindPropertyRelative("_1").ulongValue;
            var ulid = Unsafe.As<ULong2, BurstUlid>(ref ulong2);

            var str = ulid.ToString();
            var new_str = EditorGUI.TextField(textRect, ulid.ToString());
            if (new_str != str)
            {
                if (str.Length == 26)
                {
                    if (BurstUlid.TryParse(new_str, out ulid))
                    {
                        ulong2 = Unsafe.As<BurstUlid, ULong2>(ref ulid);
                        property.FindPropertyRelative("_0").ulongValue = ulong2._0;
                        property.FindPropertyRelative("_1").ulongValue = ulong2._1;
                    }
                }
            }

            if (GUI.Button(btnRect, new GUIContent("New", "Regenerate Ulid")))
            {
                ulid = BurstUlid.NewUlidCryptoRand();
                ulong2 = Unsafe.As<BurstUlid, ULong2>(ref ulid);
                property.FindPropertyRelative("_0").ulongValue = ulong2._0;
                property.FindPropertyRelative("_1").ulongValue = ulong2._1;
            }

            EditorGUI.EndProperty();
        }
    }

}
