using System;
using System.Collections.Generic;

using NatNetML;

using OptiTrack.Core;

namespace OptiTrack.NatNet {

	internal static class NatNetFrameConverter {

		internal static OptiTrackFrame ConvertFrame( FrameOfMocapData data, NatNetClientML client, IReadOnlyList<RigidBody> rigidBodyDescriptions, OptiTrackConnectionOptions options, IReadOnlyList<string> statusMessages ) {
			List<OptiTrackMarker> markers = new List<OptiTrackMarker>();
			List<OptiTrackRigidBody> rigidBodies = new List<OptiTrackRigidBody>();

			if ( options.IncludeMarkers ) {
				for ( int i = 0; i < data.nOtherMarkers; i++ ) {
					Marker marker = data.OtherMarkers[ i ];
					markers.Add( new OptiTrackMarker {
						Id = marker.ID,
						Label = marker.ID.ToString(),
						X = Math.Round( marker.x, 4 ),
						Y = Math.Round( marker.y, 4 ),
						Z = Math.Round( marker.z, 4 )
					} );
				}
			}

			if ( options.IncludeRigidBodies ) {
				for ( int i = 0; i < rigidBodyDescriptions.Count; i++ ) {
					RigidBody description = rigidBodyDescriptions[ i ];

					for ( int j = 0; j < data.nRigidBodies; j++ ) {
						RigidBodyData rigidBodyData = data.RigidBodies[ j ];
						if ( !rigidBodyData.Tracked ) {
							continue;
						}

						rigidBodies.Add( new OptiTrackRigidBody {
							Id = description.ID,
							Name = description.Name,
							IsTracked = rigidBodyData.Tracked,
							X = Math.Round( rigidBodyData.x, 4 ),
							Y = Math.Round( rigidBodyData.y, 4 ),
							Z = Math.Round( rigidBodyData.z, 4 ),
							Qx = Math.Round( rigidBodyData.qx, 6 ),
							Qy = Math.Round( rigidBodyData.qy, 6 ),
							Qz = Math.Round( rigidBodyData.qz, 6 ),
							Qw = Math.Round( rigidBodyData.qw, 6 )
						} );
					}
				}
			}

			return new OptiTrackFrame {
				FrameNumber = data.iFrame,
				TimestampSeconds = data.fTimestamp,
				LatencySeconds = client.SecondsSinceHostTimestamp( data.TransmitTimestamp ),
				IsRecording = data.bRecording,
				AssetsChanged = data.bTrackingModelsChanged,
				Markers = markers,
				RigidBodies = rigidBodies,
				StatusMessages = statusMessages
			};
		}
	}
}
