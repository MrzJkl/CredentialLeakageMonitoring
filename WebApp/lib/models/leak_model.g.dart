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
);
