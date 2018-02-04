﻿using Android.OS;
using Android.Runtime;
using Mapbox.Maps;
using System;

namespace Zebble.Plugin.MBox
{
    using View = Android.Views.View;
    public class MapViewFragment : SupportMapFragment, MapView.IOnMapChangedListener
    {
        MapView mapView;
        public MapView.IOnMapChangedListener OnMapChangedListener { get; set; }

        public bool StateSaved { get; private set; }

        public MapViewFragment(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public MapViewFragment() : base()
        {
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            mapView = view as MapView;
            mapView?.AddOnMapChangedListener(this);
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();
            mapView?.RemoveOnMapChangedListener(this);
        }

        public void OnMapChanged(int p0) => OnMapChangedListener?.OnMapChanged(p0);

        public override void OnResume()
        {
            base.OnResume();
            StateSaved = false;
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            StateSaved = true;
        }
    }
}