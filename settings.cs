# top-most EditorConfig
root = true

[*.cs]
# Encoding and EOL
charset = utf-8
end_of_line = lf
insert_final_newline = true
trim_trailing_whitespace = true

# Indentation
indent_style = space
indent_size = 4

# C# language options
dotnet_style_require_accessibility_modifiers = for_non_interface_members:suggestion
dotnet_style_qualification_for_field = false:suggestion
dotnet_style_qualification_for_property = false:suggestion
dotnet_style_qualification_for_method = false:suggestion
dotnet_style_qualification_for_event = false:suggestion

# Using directives
dotnet_sort_system_directives_first = true
dotnet_separate_import_directive_groups = true

# File\-scoped namespaces
csharp_style_namespace_declarations = file_scoped:suggestion

# Var usage
csharp_style_var_for_built_in_types = true:suggestion
csharp_style_var_when_type_is_apparent = true:suggestion
csharp_style_var_elsewhere = false:suggestion

# Expression\-bodied members
csharp_style_expression_bodied_methods = when_on_single_line:suggestion
csharp_style_expression_bodied_properties = when_on_single_line:suggestion

# Pattern matching and null checks
dotnet_style_prefer_pattern_matching = true:suggestion
dotnet_style_prefer_is_null_check_over_reference_equality_method = true:suggestion
dotnet_style_prefer_conditional_expression_over_assignment = true:suggestion

# Naming rules
dotnet_naming_rule.private_fields_should_be_camel_case.severity = suggestion
dotnet_naming_rule.private_fields_should_be_camel_case.symbols  = private_fields
dotnet_naming_rule.private_fields_should_be_camel_case.style    = camel_case_underscore

dotnet_naming_symbols.private_fields.applicable_kinds = field
dotnet_naming_symbols.private_fields.applicable_accessibilities = private
dotnet_naming_symbols.private_fields.required_modifiers =

dotnet_naming_style.camel_case_underscore.required_prefix = _
dotnet_naming_style.camel_case_underscore.capitalization = camel_case

# Newlines and braces
csharp_new_line_before_open_brace = all
csharp_new_line_between_members = true
csharp_new_line_before_members_in_object_initializers = true
csharp_new_line_before_members_in_anonymous_types = true

# Spacing
csharp_space_after_keywords_in_control_flow_statements = true
csharp_space_between_method_call_parameter_list_parentheses = false
csharp_space_between_method_declaration_parameter_list_parentheses = false
csharp_space_around_binary_operators = before_and_after

# Wrap and align
csharp_preserve_single_line_statements = true
csharp_preserve_single_line_blocks = true

# Nullable context
dotnet_style_prefer_nullable_reference_types = true:suggestion

# Using/Import cleanup
dotnet_remove_unnecessary_usings = true
dotnet_style_allow_multiple_blank_lines = false
