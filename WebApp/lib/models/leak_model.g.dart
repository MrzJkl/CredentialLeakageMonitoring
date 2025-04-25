// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'leak_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

LeakModel _$LeakModelFromJson(Map<String, dynamic> json) => LeakModel(
  id: json['id'] as String,
  emailHash: json['emailHash'] as String,
  obfuscatedPassword: json['obfuscatedPassword'] as String,
  firstSeen: DateTime.parse(json['firstSeen'] as String),
  lastSeen: DateTime.parse(json['lastSeen'] as String),
  domain: json['domain'] as String,
  associatedCustomers:
      (json['associatedCustomers'] as List<dynamic>)
          .map((e) => CustomerModel.fromJson(e as Map<String, dynamic>))
          .toList(),
);

Map<String, dynamic> _$LeakModelToJson(LeakModel instance) => <String, dynamic>{
  'id': instance.id,
  'emailHash': instance.emailHash,
  'obfuscatedPassword': instance.obfuscatedPassword,
  'firstSeen': instance.firstSeen.toIso8601String(),
  'lastSeen': instance.lastSeen.toIso8601String(),
  'domain': instance.domain,
  'associatedCustomers': instance.associatedCustomers,
};
