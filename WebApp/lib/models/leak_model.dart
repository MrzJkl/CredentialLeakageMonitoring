import 'package:json_annotation/json_annotation.dart';
import 'customer_model.dart';

part 'leak_model.g.dart';

@JsonSerializable()
class LeakModel {
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

  final List<CustomerModel> associatedCustomers;
  final String domain;
  final String emailHash;
  final DateTime firstSeen;
  final String id;
  final DateTime lastSeen;
  final String obfuscatedPassword;

  Map<String, dynamic> toJson() => _$LeakModelToJson(this);
}
