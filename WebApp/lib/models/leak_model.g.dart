// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'leak_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

LeakModel _$LeakModelFromJson(Map<String, dynamic> json) => LeakModel(
  id: json['id'] as String,
  emailHash: json['emailHash'] as String,
  firstSeen: DateTime.parse(json['firstSeen'] as String),
  lastSeen: DateTime.parse(json['lastSeen'] as String),
  domain: json['domain'] as String,
  associatedCustomers:
      (json['associatedCustomers'] as List<dynamic>)
          .map((e) => CustomerModel.fromJson(e as Map<String, dynamic>))
          .toList(),
  passwordHash: json['passwordHash'] as String,
);

Map<String, dynamic> _$LeakModelToJson(LeakModel instance) => <String, dynamic>{
  'associatedCustomers': instance.associatedCustomers,
  'domain': instance.domain,
  'emailHash': instance.emailHash,
  'passwordHash': instance.passwordHash,
  'firstSeen': instance.firstSeen.toIso8601String(),
  'id': instance.id,
  'lastSeen': instance.lastSeen.toIso8601String(),
};
