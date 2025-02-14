using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Kogane
{
    public static class ProjectBrowserInternal
    {
        private static readonly Type PROJECT_BROWSER_TYPE = typeof( Editor ).Assembly.GetType( "UnityEditor.ProjectBrowser" );

        // https://github.com/Unity-Technologies/UnityCsReference/blob/2022.1/Editor/Mono/ProjectBrowser.cs#L114
        private static readonly FieldInfo SEARCH_FIELD_TEXT_FIELD_INFO = PROJECT_BROWSER_TYPE.GetField
        (
            name: "m_SearchFieldText",
            bindingAttr: BindingFlags.NonPublic | BindingFlags.Instance
        );

        // https://github.com/Unity-Technologies/UnityCsReference/blob/2022.1/Editor/Mono/ProjectBrowser.cs#L531
        private static readonly MethodInfo SET_SEARCH_METHOD_INFO = PROJECT_BROWSER_TYPE.GetMethod
        (
            name: "SetSearch",
            bindingAttr: BindingFlags.Public | BindingFlags.Instance,
            binder: null,
            types: new[] { typeof( string ) },
            modifiers: null
        );

        public static string SearchFieldText
        {
            get
            {
                var projectBrowser = GetProjectBrowser();

                return ( string )SEARCH_FIELD_TEXT_FIELD_INFO.GetValue( projectBrowser );
            }
        }

        public static void SetSearch( string searchString )
        {
            var projectBrowser = GetProjectBrowser();

            SET_SEARCH_METHOD_INFO.Invoke( projectBrowser, new object[] { searchString } );
        }

        private static EditorWindow GetProjectBrowser()
        {
            return Resources
                    .FindObjectsOfTypeAll<EditorWindow>()
                    .FirstOrDefault( x => x.GetType().ToString() == "UnityEditor.ProjectBrowser" )
                ;
        }
    }
}