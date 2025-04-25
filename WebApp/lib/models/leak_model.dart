import 'package:json_annotation/json_annotation.dart';
import 'customer_model.dart';

part 'leak_model.g.dart';

@JsonSerializable()
class LeakModel {
  final String id;
  final String emailHash;
  final String obfuscatedPassword;
  final DateTime firstSeen;
  final DateTime lastSeen;
  final String domain;
  final List<CustomerModel> associatedCustomers;

  LeakModel({
    required this.id,
    required this.emailHash,
    required this.obfuscatedPassword,
    required this.firstSeen,
    required this.lastSeen,
    required this.domain,
    required this.associatedCustomers,
  });

  factory LeakModel.fromJson(Map<String, dynamic> json) =>
      _$LeakModelFromJson(json);

  Map<String, dynamic> toJson() => _$LeakModelToJson(this);
}
