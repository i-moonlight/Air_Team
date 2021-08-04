{{- define "Values.serviceName" -}}
{{- end -}}

{{- define "Values.host" -}}
{{- end -}}

{{- define "Values.image" -}}
{{- end -}}

{{- define "Values.labels" -}}
{{- end -}}

{{- define "labeltemplate" -}}
{{- range $key, $value := . }}
{{ $key }}: {{ $value }}
{{- end }}
{{- end }}