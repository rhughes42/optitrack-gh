# Sentry Rules For This Repository

- Sentry must be optional.
- Sentry must be disabled by default unless explicitly configured.
- Never hard-code a DSN.
- Never commit secrets.
- Never send raw OptiTrack frame data.
- Never send marker coordinates.
- Never send rigid body names.
- Never send user file paths.
- Never send IP addresses.
- Never send machine names, usernames, Rhino document names, or project/model names.
- Prefer low-cardinality tags and metrics.

Safe examples:

- `plugin_version`
- `adapter_name`
- `natnet_assembly_version`
- `rhino_major_version`
- `connection_mode`
- `frame_count`
- `marker_count`
- `rigid_body_count`
- `dropped_frame_count`
- `solve_duration_ms`
- `conversion_duration_ms`
- exception type

Unsafe examples:

- exact marker positions
- rigid body labels from Motive
- local/server IP addresses
- user paths
- file names from recordings
- Rhino document names
- raw serialized frame payloads
