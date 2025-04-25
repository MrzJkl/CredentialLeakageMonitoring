import 'package:json_annotation/json_annotation.dart';

part 'customer_model.g.dart';

@JsonSerializable()
class CustomerModel {
  CustomerModel({
    required this.id,
    required this.name,
    required this.associatedDomains,
  });

  factory CustomerModel.fromJson(Map<String, dynamic> json) =>
      _$CustomerModelFromJson(json);

  final List<String> associatedDomains;
  final String id;
  final String name;

  Map<String, dynamic> toJson() => _$CustomerModelToJson(this);
}
