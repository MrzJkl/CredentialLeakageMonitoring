// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'create_customer_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

CreateCustomerModel _$CreateCustomerModelFromJson(Map<String, dynamic> json) =>
    CreateCustomerModel(
      name: json['name'] as String,
      associatedDomains:
          (json['associatedDomains'] as List<dynamic>)
              .map((e) => e as String)
              .toList(),
    );

Map<String, dynamic> _$CreateCustomerModelToJson(
  CreateCustomerModel instance,
) => <String, dynamic>{
  'name': instance.name,
  'associatedDomains': instance.associatedDomains,
};
