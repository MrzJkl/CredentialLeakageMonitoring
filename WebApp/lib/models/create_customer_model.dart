import 'package:json_annotation/json_annotation.dart';

part 'create_customer_model.g.dart';

@JsonSerializable()
class CreateCustomerModel {
  final String name;
  final List<String> associatedDomains;

  CreateCustomerModel({required this.name, required this.associatedDomains});

  factory CreateCustomerModel.fromJson(Map<String, dynamic> json) =>
      _$CreateCustomerModelFromJson(json);

  Map<String, dynamic> toJson() => _$CreateCustomerModelToJson(this);
}
