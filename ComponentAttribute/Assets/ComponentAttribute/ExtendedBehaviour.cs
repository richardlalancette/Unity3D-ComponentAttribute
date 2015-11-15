﻿using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class ExtendedBehaviour : MonoBehaviour {

    protected virtual void Awake() {
        var cType = typeof( ComponentAttribute );
        var fields = GetType().GetFields( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )
            .Where( f => f.GetCustomAttributes( cType, true ).Length == 1 ).ToList();

        foreach ( var item in fields ) {
            var component = GetComponent( item.FieldType );
            if ( component != null ) {
                item.SetValue( this, component );
            } else {
                if ( CheckAttribute( item, cType,
                    string.Format( "Unable to load \"{0}\" on \"{1}\"", item.FieldType.Name, name ) ) )
                    return;
            }
        }

        var properties = GetType().GetProperties( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )
            .Where( p => p.GetCustomAttributes( cType, true ).Length == 1 ).ToList();

        foreach ( var item in properties ) {
            var component = GetComponent( item.PropertyType );
            if ( component != null ) {
                if ( !item.CanWrite ) {
                    if ( CheckAttribute( item, cType,
                        string.Format( "Unable to set \"{0}\" on \"{1}\"", item.Name, name ) ) )
                        return;
                } else {
                    item.SetValue( this, component, null );
                }
            } else {
                if ( CheckAttribute( item, cType,
                    string.Format( "Unable to load {0} on \"{1}\"", item.PropertyType.Name, name ) ) )
                    return;
            }
        }
    }

    private bool CheckAttribute( MemberInfo item, Type cType, string defaultMessage ) {
        var attribute = item.GetCustomAttributes( cType, true )[0] as ComponentAttribute;
        if ( attribute.DisableComponentOnError ) {
            Debug.LogErrorFormat( "{0}; Disabling {1} on \"{2}\"", defaultMessage, GetType().Name, name );
            enabled = false;
            return true;
        } else {
            Debug.LogError( defaultMessage );
        }

        return false;
    }
}