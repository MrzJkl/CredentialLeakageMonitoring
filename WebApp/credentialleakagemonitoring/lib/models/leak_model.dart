import 'package:json_annotation/json_annotation.dart';

part 'leak_model.g.dart';

@JsonSerializable(createToJson: false)
class LeakModel {
  const LeakModel({
    required this.id,
    required this.emailHash,
    required this.obfuscatedPassword,
    required this.firstSeen,
    required this.lastSeen,
  });

  factory LeakModel.fromJson(Map<String, dynamic> json) =>
      _$LeakModelFromJson(json);

  final String id;
  final String emailHash;
  final DateTime firstSeen;
  final DateTime lastSeen;
  final String obfuscatedPassword;

  // TODO: Customers
}
