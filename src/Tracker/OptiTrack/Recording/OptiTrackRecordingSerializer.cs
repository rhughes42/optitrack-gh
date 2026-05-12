using System;
using System.IO;

using Newtonsoft.Json;

namespace OptiTrack.Recording {

	public static class OptiTrackRecordingSerializer {

		public static void SaveJson( OptiTrackRecording recording, string filePath ) {
			if ( recording == null ) {
				throw new ArgumentNullException( nameof( recording ) );
			}

			if ( string.IsNullOrWhiteSpace( filePath ) ) {
				throw new ArgumentException( "File path is required.", nameof( filePath ) );
			}

			string json = JsonConvert.SerializeObject( recording, Formatting.Indented );
			File.WriteAllText( filePath, json );
		}

		public static OptiTrackRecording LoadJson( string filePath ) {
			if ( string.IsNullOrWhiteSpace( filePath ) ) {
				throw new ArgumentException( "File path is required.", nameof( filePath ) );
			}

			string json = File.ReadAllText( filePath );
			OptiTrackRecording recording = JsonConvert.DeserializeObject<OptiTrackRecording>( json );
			if ( recording == null ) {
				throw new InvalidDataException( "Recording file could not be parsed." );
			}

			if ( recording.Metadata == null ) {
				recording.Metadata = new OptiTrackRecordingMetadata();
			}

			if ( recording.Frames == null ) {
				recording.Frames = new System.Collections.Generic.List<Core.OptiTrackFrame>();
			}

			recording.Metadata.FrameCount = recording.Frames.Count;
			return recording;
		}
	}
}
